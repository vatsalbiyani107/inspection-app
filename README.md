SELECT
    s.session_id,
    s.status,
    s.login_name,
    s.host_name,
    s.program_name,
    st.text AS LastSQL
FROM sys.dm_exec_sessions s
LEFT JOIN sys.dm_exec_connections c
    ON s.session_id = c.session_id
OUTER APPLY sys.dm_exec_sql_text(c.most_recent_sql_handle) st
WHERE s.is_user_process = 1
  AND s.program_name = 'Microsoft SQL Server JDBC Driver'
ORDER BY s.session_id;
