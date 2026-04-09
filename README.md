SELECT
    r.session_id                     AS BlockedSessionID,
    r.blocking_session_id            AS BlockingSessionID,
    bs.login_name                    AS BlockedLogin,
    bs.host_name                     AS BlockedHost,
    bs.program_name                  AS BlockedProgram,
    bls.login_name                   AS BlockingLogin,
    bls.host_name                    AS BlockingHost,
    bls.program_name                 AS BlockingProgram,
    r.status                         AS BlockedRequestStatus,
    r.command                        AS BlockedCommand,
    r.wait_type,
    r.wait_time,
    blocked_sql.text                 AS BlockedSQL,
    blocking_sql.text                AS BlockingSQL
FROM sys.dm_exec_requests r
INNER JOIN sys.dm_exec_sessions bs
    ON r.session_id = bs.session_id
LEFT JOIN sys.dm_exec_sessions bls
    ON r.blocking_session_id = bls.session_id
OUTER APPLY sys.dm_exec_sql_text(r.sql_handle) blocked_sql
OUTER APPLY
(
    SELECT c.most_recent_sql_handle
    FROM sys.dm_exec_connections c
    WHERE c.session_id = r.blocking_session_id
) bhandle
OUTER APPLY sys.dm_exec_sql_text(bhandle.most_recent_sql_handle) blocking_sql
WHERE r.blocking_session_id <> 0
ORDER BY r.blocking_session_id, r.session_id;
