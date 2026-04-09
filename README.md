SELECT
    s.session_id,
    s.status,
    s.login_name,
    s.host_name,
    s.program_name,
    c.connect_time,
    c.last_read,
    c.last_write,
    st.text AS LastSQL
FROM sys.dm_exec_sessions s
LEFT JOIN sys.dm_exec_connections c
    ON s.session_id = c.session_id
OUTER APPLY sys.dm_exec_sql_text(c.most_recent_sql_handle) st
WHERE s.session_id = 67;
