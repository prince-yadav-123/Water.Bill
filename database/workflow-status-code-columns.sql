-- Adds numeric code columns for the new workflow engine.
-- Legacy text status/action columns are preserved for backward compatibility.

ALTER TABLE `ApplicationWorkflowTasks`
    ADD COLUMN `StatusCode` INT NOT NULL DEFAULT 1 AFTER `AssignedUserId`;

ALTER TABLE `ApplicationWorkflowTasks`
    ADD INDEX `IX_WorkflowTasks_AssignmentCode` (`StatusCode`, `AssignedRoleId`, `AssignedUserId`, `AssignedDepartmentId`);

ALTER TABLE `ApplicationWorkflowInstances`
    ADD COLUMN `CurrentStatusCode` INT NOT NULL DEFAULT 1 AFTER `CurrentStageId`;

ALTER TABLE `ApplicationWorkflowHistory`
    ADD COLUMN `FromStatusCode` INT NULL AFTER `StageId`,
    ADD COLUMN `ToStatusCode` INT NOT NULL DEFAULT 1 AFTER `FromStatus`,
    ADD COLUMN `ActionCode` INT NOT NULL DEFAULT 1 AFTER `ToStatus`;
