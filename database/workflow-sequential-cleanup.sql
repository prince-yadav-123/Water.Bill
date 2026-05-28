-- Repairs older workflow data where multiple stages were accidentally left pending.
-- Keeps one pending task per workflow instance, preferring the task for CurrentStageId.
-- Run once after deploying the sequential workflow fix if old parallel pending tasks exist.

DROP TEMPORARY TABLE IF EXISTS tmp_workflow_active_task;

CREATE TEMPORARY TABLE tmp_workflow_active_task AS
SELECT
    ranked.WorkflowInstanceId,
    ranked.Id AS KeepTaskId
FROM (
    SELECT
        p.WorkflowInstanceId,
        p.Id,
        ROW_NUMBER() OVER (
            PARTITION BY p.WorkflowInstanceId
            ORDER BY s.StageOrder ASC, p.AssignedOn ASC, p.Id ASC
        ) AS rn
    FROM ApplicationWorkflowTasks p
    INNER JOIN WorkflowStages s ON s.Id = p.StageId
    INNER JOIN ApplicationWorkflowInstances i ON i.Id = p.WorkflowInstanceId
    WHERE p.IsDeleted = 0
      AND p.Status = 'Pending'
      AND i.IsDeleted = 0
) ranked
WHERE ranked.rn = 1;

UPDATE ApplicationWorkflowTasks t
INNER JOIN tmp_workflow_active_task k ON k.WorkflowInstanceId = t.WorkflowInstanceId
SET
    t.Status = 'Skipped',
    t.IsActive = 0,
    t.ActionOn = COALESCE(t.ActionOn, NOW()),
    t.Remarks = COALESCE(t.Remarks, 'Closed automatically because workflow is sequential and only the current stage can remain pending.')
WHERE t.IsDeleted = 0
  AND t.Status = 'Pending'
  AND t.Id <> k.KeepTaskId;

UPDATE ApplicationWorkflowTasks t
INNER JOIN tmp_workflow_active_task k ON k.KeepTaskId = t.Id
SET t.IsActive = 1
WHERE t.IsDeleted = 0
  AND t.Status = 'Pending';

UPDATE ApplicationWorkflowInstances i
INNER JOIN tmp_workflow_active_task k ON k.WorkflowInstanceId = i.Id
INNER JOIN ApplicationWorkflowTasks t ON t.Id = k.KeepTaskId
SET i.CurrentStageId = t.StageId
WHERE i.IsDeleted = 0
  AND i.IsActive = 1;

DROP TEMPORARY TABLE IF EXISTS tmp_workflow_active_task;
