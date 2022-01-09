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
	public class PeriodicListEventArgs<T> : EventArgs
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
	public class PeriodicListExceptionEventArgs : EventArgs
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
	public class PeriodicList<T>
	{
        public delegate bool DoResponseDlgt(object sender, PeriodicListEventArgs<T> args);
        public delegate bool DoPeriodicDlgt(object sender, PeriodicListEventArgs<T> args);
        public delegate void DoExceptionDlgt(object sender, PeriodicListExceptionEventArgs args);

        public event DoResponseDlgt DoResponse;
        public event DoPeriodicDlgt DoPeriodic;
        public event DoExceptionDlgt DoException;

		protected Thread processThread;
		protected List<T> workList;
		protected EventWaitHandle waitProcess;
		protected bool stop;
        protected TimeSpan waitPeriod;
		
		/// <summary>
		/// Constructor.  Initializes the work list, wait process, and processing thread.
		/// </summary>
		public PeriodicList(TimeSpan period)
		{
			workList = new List<T>();
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
        /// This method accepts a predicate to match the item in the list and calls the OnDoResponse method.
        /// If the DoResponse handler returns true, then removes the matching item from the list.
        /// </summary>
        /// <param s="match"></param>
        public virtual void ProcessResponse(Predicate<T> match)
        {
    		lock (workList)
	    	{
                T work = workList.Find(match);
                clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "PeriodicList<T>::RemoveFromPeriodicList: processing response");
                if (OnDoResponse(new PeriodicListEventArgs<T>(work)) == true)
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
                            foreach (T work in workList)
                            {
                                try
                                {
                                    // Try processing it.
                                    if (OnDoPeriodic(new PeriodicListEventArgs<T>(work)) == true)
                                        workList.Remove(work);
                                    clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "PeriodicList<T>::ProcessPeriodicList: ");
                                }
                                catch (Exception e)
                                {
                                    // Oops, inform application of a work error.
                                    OnWorkException(new PeriodicListExceptionEventArgs(e));
                                    workList.Remove(work);
                                    clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "PeriodicList<T>::ProcessPeriodicList: reporting exception: " + e.ToString());
                                }
                            }
                        }
					}

                    Thread.Sleep(waitPeriod);

				} while (haveWork);	// continue processing if there was work.
			}
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
        /// Override this method if you want to handle response actions in a derived class.  This method
        /// calls any events wired in to the DoResponse event.
        /// </summary>
        /// <param s="workEventArgs"></param>
        protected virtual bool OnDoResponse(PeriodicListEventArgs<T> workEventArgs)
        {
            if (DoResponse != null)
            {
                clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "PeriodicList<T>::OnDoResponse: calling response handler");
                return DoResponse(this, workEventArgs);
            }
            else
                return true;
        }

        /// <summary>
		/// Override this method if you want to handle work exceptions in a derived class.
		/// This method calls any events wired in to the DoException event.
		/// </summary>
		/// <param s="workExceptionArgs"></param>
		protected virtual void OnWorkException(PeriodicListExceptionEventArgs workExceptionArgs)
		{
            if (DoException != null)
            {
                DoException(this, workExceptionArgs);
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "PeriodicList<T>::OnWorkException: calling exception handler");
        }
	}
}
