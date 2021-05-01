using System;
using System.Collections;
using System.Threading;

namespace Automation
{
    /// <summary>
    /// Double-ended Queue (deque) that supports bounded or buffer style insertions with blocking.
    /// BlockingDeque is internally synchronized (thread-safe) and designed for concurrent producers and consumer threads.
    ///
    /// Blocking supports "ping-pong" behavior between consumer and producer threads. Consumer thread(s) will block on
    /// Dequeue operations until another thread performs a Enqueue
    /// operation, at which point the first scheduled consumer thread will be unblocked and get the
    /// current T.  Producer thread(s) will block on Enqueue operations until another
    /// consumer thread calls Dequeue to free a queue slot, at which point the first scheduled producer
    /// thread will be unblocked to finish its Enqueue operation.
    ///
    /// Currently does not support insert at head.
    /// </summary>
    internal class BlockingDeque<T>
    {
        private bool overwrite;

        /// <summary>
        /// Create instance of a BlockingDeque with bounded number of elements. If the deque is full,
        /// an additional Enqueue operation will overwrite the head element in the deque.
        /// When the BlockingDeque is empty, calls to Dequeue, Head, or Tail will block until another
        /// thread makes a call to Enqueue.
        /// </summary>
        /// <param name="exponent">Exponent of 2^x sets size of queue</param>
        public BlockingDeque(int exponent)
            : this(exponent, true)
        {
        }

        /// <summary>
        /// Create instance of a BlockingDeque with bounded number of elements. If the deque is full,
        /// an additional Enqueue operation will either overwrite the head element in the deque or block until a consumer thread calls Dequeue to free a slot.
        /// When the BlockingDeque is empty, calls to Dequeue, Head, or Tail will block until another
        /// thread makes a call to Enqueue.
        /// </summary>
        /// <param name="exponent">Deque capacity will be power of two less one (2 ^ capacity - 1)</param>
        /// <param name="overwriting">Inidicates whether Enqueue operations overwrite the head item when capacity has been met</param>
        public BlockingDeque(int exponent, bool overwriting)
        {
            if (exponent < 0)
                throw new ArgumentOutOfRangeException("Exponent must be zero or greater.");

            syncRoot = new object();
            double requestedSize = Math.Pow((double)2, (double)exponent);
            if (requestedSize > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("2^size is greater than maximum allowed value.");
            }
            this.N = (int)requestedSize;

            buffer = new T[N];
            length = 0;
            head = 0;
            tail = 0;
            overwrite = overwriting;
        }

        /// <summary>
		/// Gets the T values currently in the queue.  If queue is empty, this
		/// will return a zero length array.  The returned array length can be
		/// 0 to Size.  This method does not modify the queue, but returns a shallow copy
		/// of the queue buffer containing the objects contained in the queue.
		/// </summary>
        public T[] Values
        {
            get
            {
                T[] destinationArray = new T[length];
                if (length != 0)
                {
                    if (head < tail)
                    {
                        Array.Copy(buffer, head, destinationArray, 0, length);
                    }
                    else
                    {
                        Array.Copy(buffer, head, destinationArray, 0, N - head);
                        Array.Copy(buffer, 0, destinationArray, N - head, tail);
                    }
                }
                return destinationArray;
            }
        }

        /// <summary>
        /// Removes all objects from the deque.
        /// </summary>
        /// <remarks>
        /// Count is set to zero. Size does not change.
        /// </remarks>
        public void Clear()
        {
            lock (syncRoot)
            {
                if (head < tail)
                {
                    Array.Clear(buffer, head, length);
                }
                else
                {
                    Array.Clear(buffer, head, N - head);
                    Array.Clear(buffer, 0, tail);
                }
                head = 0;
                tail = 0;
                length = 0;
            }
        }
         
        /// <summary>
        /// Removes and returns item at the tail of the queue if at least one item is enqueued. Otherwise, resulting output is default(T).
        /// </summary>
        /// <returns>True if an item was removed from queue tail</returns>
        public bool TryUndoEnqueue(out T value)
        {
            lock (syncRoot)
            {
                if (length == 0)
                {
                    value = default(T);
                    return false;
                }

                if (tail > 0)
                    tail = (tail - 1) % N;
                else
                    tail = N - 1;

                value = buffer[tail];
                buffer[tail] = default(T);

                length--;

                if (length == (N - 1) - 1)	// Could have blocking Enqueue thread(s).
                    Monitor.PulseAll(syncRoot);
            }

            return true;
        }

        /// <summary>
        /// Removes and returns item at the end of the queue. Throws an exception if deque is empty.
        /// </summary>
        /// <exception cref="InvalidOpertionException">The queue is empty.</exception>
        public T UndoEnqueue()
        {
            lock (syncRoot)
            {
                if (length == 0)
                {
                    throw new InvalidOperationException("Deque is empty.");
                }

                if (tail > 0)
                    tail = (tail - 1) % N;
                else
                    tail = N - 1;

                T value = buffer[tail];
                buffer[tail] = default(T);

                length--;

                if (length == (N - 1) - 1)	// Could have blocking Enqueue thread(s).
                    Monitor.PulseAll(syncRoot);

                return value;
            }
        }

        #region Fields

        protected T[] buffer;			    // Buffer used to store queue objects with max "Size".
        protected int length;				// Current number of elements in the queue.
        protected int N;
        protected int head;					// Index of slot for T to remove on next Dequeue. 
        protected int tail;					// Index of slot for next Enqueue T.
        protected readonly object syncRoot;	// Object used to synchronize the queue.

        #endregion Fields

        #region Public Methods

        /// <summary>
        /// Returns n-th item in queue starting from head
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                if (length == 0)
                {
                    throw new InvalidOperationException("Deque is empty.");
                }
                else if (index + 1 > length || index < 0)
                {
                    throw new ArgumentException("Index is out of range.");
                }

                int position = (head + index) % N;
                T value = buffer[position];

                return value;
            }
        }

        /// <summary>
        /// Removes and returns the T at the beginning of the deque.
        /// If queue is empty, method will block until another thread calls one of
        /// the Enqueue methods.   This method will wait "Timeout.Infinite" until another
        /// thread Enqueues and T.
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            bool timeout = false;
            return Dequeue(Timeout.Infinite, out timeout);
        }

        /// <summary>
        /// Removes and returns the T at the beginning of the deque.
        /// If queue is empty, method will block until another thread calls one of
        /// the Enqueue methods or millisecondsTimeout expires.
        /// If timeout, method will throw QueueTimeoutException.
        /// </summary>
        /// <returns>The T that is removed from the beginning of the deque.</returns>
        public T Dequeue(int millisecondsTimeout, out bool timeout)
        {
            T value;
            lock (syncRoot)
            {
                while (length == 0)
                {
                    try
                    {
                        if (!Monitor.Wait(syncRoot, millisecondsTimeout))
                        {
                            timeout = true;
                            return default(T);
                        }
                    }
                    catch
                    {
                        Monitor.PulseAll(syncRoot);
                        throw;
                    }
                }
                value = buffer[head];
                buffer[head] = default(T);
                head = (head + 1) % N;
                length--;

                if (length == (N - 1) - 1)	// Could have blocking Enqueue thread(s).
                    Monitor.PulseAll(syncRoot);
            }
            timeout = false;
            return value;
        }

        /// <summary>
        /// Adds an T to the end of the queue. If queue is full, this method will
        /// block until another thread calls one of the Dequeue methods.  This method will wait
        /// "Timeout.Infinite" until queue has a free slot.
        /// </summary>
        /// <param name="value"></param>
        public virtual void Enqueue(T value)
        {
            bool timeout = false;
            Enqueue(value, Timeout.Infinite, out timeout);
        }

        /// <summary>
        /// Adds an T to the end of the queue. If queue is full, this method will
        /// block until another thread calls one of the Dequeue methods or millisecondsTimeout
        /// expires.  If timeout, method will return value in timeout out parameter.
        /// </summary>
        /// <param name="value"></param>
        public virtual void Enqueue(T value, int millisecondsTimeout, out bool timeout)
        {
            lock (syncRoot)
            {
                while (length == (N - 1))
                {
                    if (overwrite)
                    {
                        //dequeue head
                        buffer[head] = default(T);
                        head = (head + 1) % N;

                        // we'll add this back below
                        length--;
                    }
                    else
                    {
                        try
                        {
                            if (!Monitor.Wait(syncRoot, millisecondsTimeout))
                            {
                                timeout = true;
                                return;
                            }
                        }
                        catch
                        {
                            // Monitor exited with exception.  Could be owner thread of monitor
                            // T was terminated or timeout on wait.  Pulse any/all waiting
                            // threads to ensure we don't get any "live locked" producers.
                            Monitor.PulseAll(syncRoot);
                            throw;
                        }
                    }
                }
                buffer[tail] = value;
                tail = (tail + 1) % N;
                length++;

                if (length == 1)	// Could have blocking Dequeue thread(s).
                    Monitor.PulseAll(syncRoot);

                timeout = false;
                return;
            }
        }

        public T FromTail(int index)
        {
            int position;
            if (tail > 0)
                position = (tail - index) % N;
            else
                position = N - 1;

            T value = buffer[position];
            return value;
        }

        /// <summary>
        /// Returns the T at the beginning of the deque without removing it.
        /// </summary>
        /// <returns>The T at the beginning of the deque.</returns>
        /// <remarks>
        /// Head waits indefinately for a new item to be enqueued if the deque is empty.
        /// </remarks>
        /// <exception cref="InvalidOpertionException">The queue is empty.</exception>
        public T Head()
        {
            bool timeout = false;
            return Head(Timeout.Infinite, out timeout);
        }

        /// <summary>
        /// Returns the T at the beginning of the deque without removing it.
        /// </summary>
        /// <returns>The T at the beginning of the deque.</returns>
        /// <remarks>
        /// Head waits for the specified time for a new item to be enqueued if the deque is empty.
        /// </remarks>
        public T Head(int millisecondsTimeout, out bool timeout)
        {
            T value;
            lock (syncRoot)
            {
                while (length == 0)
                {
                    try
                    {
                        if (!Monitor.Wait(syncRoot, millisecondsTimeout))
                        {
                            timeout = true;
                            return default(T);
                        }
                    }
                    catch
                    {
                        Monitor.PulseAll(syncRoot);
                        throw;
                    }
                }
                value = buffer[head];
            }

            timeout = false;
            return value;
        }

        /// <summary>
        /// Returns the T at the end of the deque without removing it.
        /// </summary>
        /// <returns>The T at the end of the deque.</returns>
        /// <remarks>
        /// Tail waits indefinately for a new item to be enqueued if the deque is empty.
        /// </remarks>
        /// <exception cref="InvalidOpertionException">The queue is empty.</exception>
        public T Tail()
        {
            bool timeout = false;
            return Tail(Timeout.Infinite, out timeout);
        }

        /// <summary>
        /// Returns the T at the end of the deque without removing it.
        /// </summary>
        /// <returns>The T at the end of the deque.</returns>
        /// <remarks>
        /// Tail waits for the specified time for a new item to be enqueued if the deque is empty.
        /// </remarks>
        public T Tail(int millisecondsTimeout, out bool timeout)
        {
            T value;
            lock (syncRoot)
            {
                while (length == 0)
                {
                    try
                    {
                        if (!Monitor.Wait(syncRoot, millisecondsTimeout))
                        {
                            timeout = true;
                            return default(T);
                        }
                    }
                    catch
                    {
                        Monitor.PulseAll(syncRoot);
                        throw;
                    }
                }

                if (tail > 0)
                    value = buffer[(tail - 1) % N];
                else
                    value = buffer[N - 1];
            }

            timeout = false;
            return value;
        }

        /// <summary>
        /// Non-blocking version of Dequeue.  Will return false if queue is empty and set
        /// value to null, otherwise will return true and set value to the dequeued T.
        /// </summary>
        /// <param name="value">The T that is removed from the beginning of the deque or null if empty.</param>
        /// <returns>true if successfull, otherwise false.</returns>
        public bool TryDequeue(out T value)
        {
            lock (syncRoot)
            {
                if (length == 0)
                {
                    value = default(T);
                    return false;
                }

                value = buffer[head];
                buffer[head] = default(T);
                head = (head + 1) % N;
                length--;

                if (length == (N - 1) - 1)	// Could have blocking Enqueue thread(s).
                    Monitor.PulseAll(syncRoot);
            }
            return true;
        }

        /// <summary>
        /// Non-blocking version of Enqueue().  If Enqueue is successfull, this will
        /// return true; otherwise false if queue is full.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>true if successfull, otherwise false.</returns>
        public bool TryEnqueue(T value)
        {
            lock (syncRoot)
            {
                if (length == (N - 1) && !overwrite)
                    return false;

                buffer[tail] = value;
                tail = (tail + 1) % N;
                length++;

                if (length == 1)	// Could have blocking Dequeue thread(s).
                    Monitor.PulseAll(syncRoot);
            }
            return true;
        }

        /// <summary>
        /// Returns the T at the beginning of the deque without removing it.
        /// Similar to the Head method, however this method will not wait if
        /// deque is empty and will instead return false.
        /// </summary>
        /// <param name="value">The T at the beginning of the deque or null if empty.</param>
        /// <returns>The T at the beginning of the deque.</returns>
        public bool TryHead(out T value)
        {
            lock (syncRoot)
            {
                if (length == 0)
                {
                    value = default(T);
                    return false;
                }
                value = buffer[head];
            }
            return true;
        }

        /// <summary>
        /// Returns the T at the end of the deque without removing it.
        /// Similar to the Tail method, however this method will not wait if
        /// deque is empty and will instead return false.
        /// </summary>
        /// <param name="value">The T at the end of the deque or null if empty.</param>
        /// <returns>The T at the end of the deque.</returns>
        public bool TryTail(out T value)
        {
            lock (syncRoot)
            {
                if (length == 0)
                {
                    value = default(T);
                    return false;
                }

                if (tail > 0)
                    value = buffer[(tail - 1) % N];
                else
                    value = buffer[N - 1];
            }
            return true;
        }

        #endregion Public Methods

        #region ICollection Members

        /// <summary>
        /// Returns the max elements allowed in the queue before blocking Enqueue
        /// operations.  This is the size set in the constructor.
        /// </summary>
        public int Capacity
        {
            get { return N - 1; }
        }

        /// <summary>
        /// Gets the number of elements contained in the deque.
        /// </summary>
        public int Count
        {
            get { lock (syncRoot) { return length; } }
        }

        /// <summary>
        /// Gets a value indicating whether access to the deque is synchronized (thread-safe).
        /// </summary>
        public bool IsSynchronized
        {
            get { return true; }
        }

        /// <summary>
        /// Gets an T that can be used to synchronize access to the deque.
        /// </summary>
        public object SyncRoot
        {
            get { return this.syncRoot; }
        }

        /// <summary>
        /// Copies the deque elements to an existing one-dimensional Array,
        /// starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from the deque. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins. </param>
        public void CopyTo(Array array, int index)
        {
            T[] tmpArray = Values;
            tmpArray.CopyTo(array, index);
        }

        #endregion ICollection Members

        #region IEnumerable Members

        /// <summary>
        /// GetEnumerator not implemented.  You can't enumerate the active queue
        /// as you would an array as it is dynamic with active gets and puts.  You could
        /// if you locked it first and unlocked after enumeration, but that does not
        /// work well for GetEnumerator.  The recommended method is to Get Values
        /// and enumerate the returned array copy.  That way the queue is locked for
        /// only a short time and a copy returned so that can be safely enumerated using
        /// the array's enumerator.  You could also create a custom enumerator that would
        /// dequeue the objects until empty queue, but that is a custom need.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException("Not Implemented.");
        }

        #endregion IEnumerable Members
    }
}