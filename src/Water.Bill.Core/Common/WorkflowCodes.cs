namespace Water.Bill.Core.Common;

public static class WorkflowCodes
{
    public static class TaskStatus
    {
        public const int Pending = 1;
        public const int Approved = 2;
        public const int Rejected = 3;
        public const int SentBackToApplicant = 4;
        public const int SentBackToPreviousStage = 5;
        public const int Forwarded = 6;
        public const int Skipped = 7;
    }

    public static class InstanceStatus
    {
        public const int Pending = 1;
        public const int UnderReview = 2;
        public const int Approved = 3;
        public const int Rejected = 4;
        public const int SentBackToApplicant = 5;
        public const int SentBackToPreviousStage = 6;
        public const int FinalConsumerCreated = 7;
        public const int Completed = 8;
    }

    public static class ActionCode
    {
        public const int WorkflowStarted = 1;
        public const int AcceptMoveNext = 2;
        public const int FinalApproval = 3;
        public const int Reject = 4;
        public const int SendBackToApplicant = 5;
        public const int SendBackToPreviousStage = 6;
        public const int ForwardToUser = 7;
        public const int StageAssigned = 8;
        public const int FinalConsumerCreated = 9;
    }
}
