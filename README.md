SELECT
    t.name AS TriggerName,
    m.definition
FROM sys.triggers t
INNER JOIN sys.sql_modules m
    ON t.object_id = m.object_id
WHERE m.definition LIKE '%SLAB_LMS_DETAILS_L2%';
