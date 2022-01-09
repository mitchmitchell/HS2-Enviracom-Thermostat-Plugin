using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace HSPI_ENVIRACOM_MANAGER
{
	/// <summary>
	/// Wrapper class for the work to be done.
	/// </summary>
	/// <typeparam s="T"></typeparam>
	class PeriodicListEventArgs<T> : EventArgs
	{
		protected T work;
		
		public T Work
		{
			get { return work; }
		}

		public PeriodicListEventArgs(T work)
		{
			this.work = work;
		}
	}

	/// <summary>
	/// Wrapper class for recording an exception that occurred while processing the work packet.
	/// </summary>
	class PeriodicListExceptionEventArgs : EventArgs
	{
		protected Exception exception;

		public Exception Exception
		{
			get { return exception; }
		}

		public PeriodicListExceptionEventArgs(Exception e)
		{
			exception = e;
		}
	}

	/// <summary>
	/// A re-usable class for processing items repeatedly in a worker thread until the item indicates work is done.
	/// </summary>
	class PeriodicList<T>
	{
        public delegate bool DoImmediateDlgt(object sender, PeriodicListEventArgs<T> args);
        public delegate bool DoPeriodicDlgt(object sender, PeriodicListEventArgs<T> args);
        public delegate void DoExceptionDlgt(object sender, PeriodicListExceptionEventArgs args);

        public event DoImmediateDlgt DoImmediate;
        public event DoPeriodicDlgt DoPeriodic;
        public event DoExceptionDlgt DoException;
        protected Predicate<T> checkMatch;
		protected Thread processThread;
		protected List<T> workList;
		protected EventWaitHandle waitProcess;
		protected bool stop;
        protected TimeSpan waitPeriod;
		
		/// <summary>
		/// Constructor.  Initializes the work list, wait process, and processing thread.
		/// </summary>
        public PeriodicList(TimeSpan period, Predicate<T> match)
		{
			workList = new List<T>();
            checkMatch = match;
			waitProcess = new EventWaitHandle(false, EventResetMode.AutoReset);
            waitPeriod = period;
			processThread = new Thread(new ThreadStart(ProcessPeriodicList));
			processThread.IsBackground = true;
			processThread.Start();
		}

		/// <summary>
		/// Enqueue a work item.
		/// </summary>
		/// <param s="work"></param>
		public void AddToPeriodicList(T work)
		{
			lock (workList)
			{
				workList.Add(work);
			}

			waitProcess.Set();

		}
        /// <summary>
        /// This method accepts a predicate to match the item in the list and calls the OnDoImmediate method.
        /// If the DoImmediate handler returns true, then removes the matching item from the list.
        /// </summary>
        /// <param s="match"></param>
        public virtual void ProcessImmediate(Predicate<T> match)
        {
    		lock (workList)
	    	{
                T work = workList.Find(match);
                clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "PeriodicList<T>::RemoveFromPeriodicList: processing response");
                if (OnDoImmediate(new PeriodicListEventArgs<T>(work)) == true)
                    workList.Remove(work);
            }
        }

		/// <summary>
		/// Stop the work processing thread.
		/// </summary>
		public void Stop()
		{
			stop = true;
			waitProcess.Set();
		}

		/// <summary>
		/// Process queued work.
		/// </summary>
		protected void ProcessPeriodicList()
		{
			while (!stop)
			{
				// Wait for some work.
                clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "PeriodicList<T>::ProcessPeriodicList: waiting for work");
                waitProcess.WaitOne();
                if (!stop)
                {
				    bool haveWork = false;

				    // Finish remaining work before stopping.
				    do
				    {
					    // Assume no work.
					    haveWork = false;

					    // Prevent enqueing from a different thread.
					    lock (workList)
					    {
						    // Do we have work?  This might be 0 if stopping or if all work is processed.
						    if (workList.Count > 0)
						    {
                                // Yes, we have work.
                                haveWork = true;
                                // Get the work.
                                // Need to work on this logic.   I would prefer that the PeriodicList class handle the removal of items from the list when appropriate rather than requiring a special
                                // checkMatch delegate to be passed in.   The logic should rely on the return value from OnDoPeriodic rather than the checkMatch parameter.  Right now this logic is
                                // a hack since the workList cannot be modifed inside the foreach block.  We don't want to do things like making a copy of the list each iteration since the tick rate
                                // for periodic can be quite high.
                                foreach (T work in workList)
                                {
                                    try
                                    {
                                        // Try processing it.
                                        if (OnDoPeriodic(new PeriodicListEventArgs<T>(work)))
                                            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "PeriodicList<T>::ProcessPeriodicList: " + work.ToString() + " should be marked for removal");
                                    }
                                    catch (Exception e)
                                    {
                                        // Oops, inform application of a work error.
                                        OnDoException(new PeriodicListExceptionEventArgs(e));
                                        clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "PeriodicList<T>::ProcessPeriodicList: reporting exception: " + e.ToString());
                                    }
                                }
                                workList.RemoveAll(checkMatch);
                            }
					    }

                        Thread.Sleep(waitPeriod);

				    } while (haveWork);	// continue processing if there was work.
                }   // if not stopping
			}   // while not stopping
		}

        /// <summary>
        /// Override this method if you want to handle periodic actions in a derived class.  This method
        /// calls any events wired in to the DoPeriodic event.
        /// </summary>
        /// <param s="workEventArgs"></param>
        protected virtual bool OnDoPeriodic(PeriodicListEventArgs<T> workEventArgs)
        {
            if (DoPeriodic != null)
            {
                clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "PeriodicList<T>::OnDoPeriodic: calling periodic handler");
                return DoPeriodic(this, workEventArgs);
            }
            else
                return true;
        }

        /// <summary>
        /// Override this method if you want to handle immediate actions in a derived class.  This method
        /// calls any events wired in to the DoImmediate event.
        /// </summary>
        /// <param s="workEventArgs"></param>
        protected virtual bool OnDoImmediate(PeriodicListEventArgs<T> workEventArgs)
        {
            if (DoImmediate != null)
            {
                clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "PeriodicList<T>::OnDoImmediate: calling immediate handler");
                return DoImmediate(this, workEventArgs);
            }
            else
                return true;
        }

        /// <summary>
		/// Override this method if you want to handle work exceptions in a derived class.
		/// This method calls any events wired in to the DoException event.
		/// </summary>
		/// <param s="workExceptionArgs"></param>
		protected virtual void OnDoException(PeriodicListExceptionEventArgs workExceptionArgs)
		{
            if (DoException != null)
            {
                DoException(this, workExceptionArgs);
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "PeriodicList<T>::OnWorkException: calling exception handler");
        }
	}
}
