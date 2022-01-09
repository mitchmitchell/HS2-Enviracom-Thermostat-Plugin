/*
Copyright (c) 2006, Marc Clifton
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list
  of conditions and the following disclaimer. 

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or other
  materials provided with the distribution. 
 
* Neither the s Marc Clifton nor the names of contributors may be
  used to endorse or promote products derived from this software without specific
  prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Collections.Generic;
using System.Threading;

namespace HSPI_ENVIRACOM_MANAGER
{
	/// <summary>
	/// Wrapper class for the work to be done.
	/// </summary>
	/// <typeparam s="T"></typeparam>
	public class ProcessingQueueEventArgs<T> : EventArgs
	{
		protected T work;
		
		public T Work
		{
			get { return work; }
		}

		public ProcessingQueueEventArgs(T work)
		{
			this.work = work;
		}
	}

	/// <summary>
	/// Wrapper class for recording an exception that occurred while processing the work packet.
	/// </summary>
	public class ProcessingQueueExceptionEventArgs : EventArgs
	{
		protected Exception exception;

		public Exception Exception
		{
			get { return exception; }
		}

		public ProcessingQueueExceptionEventArgs(Exception e)
		{
			exception = e;
		}
	}

	/// <summary>
	/// A re-usable class for processing items in a worker thread as they are queued.
	/// </summary>
	public class ProcessingQueue<T>
	{
		public delegate void DoWorkDlgt(object sender, ProcessingQueueEventArgs<T> args);
		public delegate void WorkExceptionDlgt(object sender, ProcessingQueueExceptionEventArgs args);

		public event DoWorkDlgt DoWork;
		public event WorkExceptionDlgt WorkException;

		protected Thread processThread;
		protected Queue<T> workQueue;
		protected EventWaitHandle waitProcess;
		protected bool stop;
		
		/// <summary>
		/// Constructor.  Initializes the work queue, wait process, and processing thread.
		/// </summary>
		public ProcessingQueue()
		{
			workQueue = new Queue<T>();
			waitProcess = new EventWaitHandle(false, EventResetMode.AutoReset);
			processThread = new Thread(new ThreadStart(ProcessQueueWork));
			processThread.IsBackground = true;
			processThread.Start();
		}

		/// <summary>
		/// Enqueue a work item.
		/// </summary>
		/// <param s="work"></param>
		public void QueueForWork(T work)
		{
			lock (workQueue)
			{
				workQueue.Enqueue(work);
			}

			waitProcess.Set();

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
		protected void ProcessQueueWork()
		{
			while (!stop)
			{
				// Wait for some work.
				waitProcess.WaitOne();
				bool haveWork;

				// Finish remaining work before stopping.
				do
				{
					// Initialize to the default work value.
					T work = default(T);
					// Assume no work.
					haveWork = false;

					// Prevent enqueing from a different thread.
					lock (workQueue)
					{
						// Do we have work?  This might be 0 if stopping or if all work is processed.
						if (workQueue.Count > 0)
						{
							// Get the work.
							work = workQueue.Dequeue();
							// Yes, we have work.
							haveWork = true;
						}
					}

					// If we have work...
					if (haveWork)
					{
						try
						{
							// Try processing it.
							OnDoWork(new ProcessingQueueEventArgs<T>(work));
						}
						catch (Exception e)
						{
							// Oops, inform application of a work error.
							OnWorkException(new ProcessingQueueExceptionEventArgs(e));
						}
					}

				} while (haveWork);	// continue processing if there was work.
			}
		}

		/// <summary>
		/// Override this method if you want to handle work in a derived class.  This method
		/// calls any events wired in to the DoPeriodic event.
		/// </summary>
		/// <param s="workEventArgs"></param>
		protected virtual void OnDoWork(ProcessingQueueEventArgs<T> workEventArgs)
		{
			if (DoWork != null)
			{
				DoWork(this, workEventArgs);
			}
		}

		/// <summary>
		/// Override this method if you want to handle work exceptions in a derived class.
		/// This method calls any events wired in to the DoException event.
		/// </summary>
		/// <param s="workExceptionArgs"></param>
		protected virtual void OnWorkException(ProcessingQueueExceptionEventArgs workExceptionArgs)
		{
			if (WorkException != null)
			{
				WorkException(this, workExceptionArgs);
			}
		}
	}
}
