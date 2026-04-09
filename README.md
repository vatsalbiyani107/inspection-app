SELECT
    t.TableName,
    l.request_session_id AS LockedSessionID,
    wt.blocking_session_id AS BlockingSessionID,
    l.resource_type,
    l.request_mode,
    l.request_status,
    s.login_name,
    s.host_name,
    s.program_name,
    r.status AS RequestStatus,
    r.command,
    r.wait_type,
    r.wait_time,
    r.total_elapsed_time,
    txt.text AS RunningSQL
FROM sys.dm_tran_locks l
INNER JOIN
(
    SELECT
        p.hobt_id,
        OBJECT_NAME(p.object_id) AS TableName
    FROM sys.partitions p
) t
    ON l.resource_associated_entity_id = t.hobt_id
LEFT JOIN sys.dm_exec_sessions s
    ON l.request_session_id = s.session_id
LEFT JOIN sys.dm_exec_requests r
    ON l.request_session_id = r.session_id
LEFT JOIN sys.dm_os_waiting_tasks wt
    ON l.request_session_id = wt.session_id
OUTER APPLY sys.dm_exec_sql_text(r.sql_handle) txt
WHERE t.TableName = 'YourTableName'
ORDER BY l.request_session_id;
