using System;
using System.IO ;
using System.Collections.Generic;
using System.Text;

namespace WSNTBS
{
    // an abstract class which works link an interface
    // it define some function and the derived class must implement them
    abstract class  TBDatabaseAgent
    {
        public abstract TBExpJob[] GetQueuedJobs();
        public abstract void SetJobStatusDone(int inExpID);
        public abstract bool GetImageFileAndSaveInLocal(string inExpUser, 
                                                        string inFilename, 
                                                        string inLocalDirectory);
        public abstract void WriteUARTLogs(int inExpID, 
                                           int inNodeID, 
                                           string inLogs);
        public abstract void SetJobStatus(int inExpID,
                                          bool inWorking);
        public abstract void SetJobStatusCanceled(int inExpID);
        public abstract bool IsJobCanceled(int inExpID);
        public abstract bool IsJobDone(int inExpID);
        public abstract void UpdateNodeStatus();
        public abstract void DeleteExpTempFile(int inExpID);
    }
}
