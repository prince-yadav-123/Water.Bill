-- Backfills numeric workflow code columns for existing workflow data.
-- Keeps legacy text columns intact and only populates the new code columns.

UPDATE `ApplicationWorkflowTasks`
SET `StatusCode` = CASE
    WHEN `Status` IN ('Pending') THEN 1
    WHEN `Status` IN ('Approved', 'Approve', 'Accepted') THEN 2
    WHEN `Status` IN ('Rejected', 'Reject') THEN 3
    WHEN `Status` IN ('SentBackToApplicant', 'CorrectionRequired') THEN 4
    WHEN `Status` IN ('SentBackToPrevious', 'SentBackToPreviousStage') THEN 5
    WHEN `Status` IN ('Forwarded') THEN 6
    WHEN `Status` IN ('Skipped') THEN 7
    ELSE COALESCE(`StatusCode`, 1)
END
WHERE `Id` > 0
  AND `IsDeleted` = 0;

UPDATE `ApplicationWorkflowInstances`
SET `CurrentStatusCode` = CASE
    WHEN `CurrentStatus` IN ('Pending') THEN 1
    WHEN `CurrentStatus` IN ('UnderReview') THEN 2
    WHEN `CurrentStatus` IN ('Approved') THEN 3
    WHEN `CurrentStatus` IN ('Rejected') THEN 4
    WHEN `CurrentStatus` IN ('SentBackToApplicant', 'CorrectionRequired') THEN 5
    WHEN `CurrentStatus` IN ('SentBackToPreviousStage') THEN 6
    WHEN `CurrentStatus` IN ('FinalConsumerCreated') THEN 7
    WHEN `CurrentStatus` IN ('Completed') THEN 8
    ELSE COALESCE(`CurrentStatusCode`, 1)
END
WHERE `Id` > 0
  AND `IsDeleted` = 0;

UPDATE `ApplicationWorkflowHistory`
SET
    `FromStatusCode` = CASE
        WHEN `FromStatus` IN ('Pending') THEN 1
        WHEN `FromStatus` IN ('UnderReview') THEN 2
        WHEN `FromStatus` IN ('Approved') THEN 3
        WHEN `FromStatus` IN ('Rejected') THEN 4
        WHEN `FromStatus` IN ('SentBackToApplicant', 'CorrectionRequired') THEN 5
        WHEN `FromStatus` IN ('SentBackToPreviousStage') THEN 6
        WHEN `FromStatus` IN ('FinalConsumerCreated') THEN 7
        WHEN `FromStatus` IN ('Completed') THEN 8
        ELSE `FromStatusCode`
    END,
    `ToStatusCode` = CASE
        WHEN `ToStatus` IN ('Pending') THEN 1
        WHEN `ToStatus` IN ('UnderReview') THEN 2
        WHEN `ToStatus` IN ('Approved') THEN 3
        WHEN `ToStatus` IN ('Rejected') THEN 4
        WHEN `ToStatus` IN ('SentBackToApplicant', 'CorrectionRequired') THEN 5
        WHEN `ToStatus` IN ('SentBackToPreviousStage') THEN 6
        WHEN `ToStatus` IN ('FinalConsumerCreated') THEN 7
        WHEN `ToStatus` IN ('Completed') THEN 8
        ELSE COALESCE(`ToStatusCode`, 1)
    END,
    `ActionCode` = CASE
        WHEN `Action` IN ('WorkflowStarted') THEN 1
        WHEN `Action` IN ('AcceptMoveNext', 'MoveNext', 'Approved') THEN 2
        WHEN `Action` IN ('FinalApproval') THEN 3
        WHEN `Action` IN ('Reject', 'Rejected') THEN 4
        WHEN `Action` IN ('SendBackToApplicant', 'CorrectionRequired', 'SendCorrection', 'Correction') THEN 5
        WHEN `Action` IN ('SendBackToPrevious', 'SendBackPrevious') THEN 6
        WHEN `Action` IN ('ForwardToUser', 'ForwardUser', 'Forward To Specific User') THEN 7
        WHEN `Action` IN ('StageAssigned') THEN 8
        WHEN `Action` IN ('FinalConsumerCreated') THEN 9
        ELSE COALESCE(`ActionCode`, 8)
    END
WHERE `Id` > 0;
