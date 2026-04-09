SELECT
    r.session_id,
    r.blocking_session_id,
    r.status,
    r.command,
    r.wait_type,
    r.wait_time,
    s.login_name,
    s.host_name,
    s.program_name,
    st.text AS RunningSQL
FROM sys.dm_exec_requests r
INNER JOIN sys.dm_exec_sessions s
    ON r.session_id = s.session_id
OUTER APPLY sys.dm_exec_sql_text(r.sql_handle) st
WHERE r.blocking_session_id <> 0
   OR r.session_id IN
   (
       SELECT blocking_session_id
       FROM sys.dm_exec_requests
       WHERE blocking_session_id <> 0
   );
