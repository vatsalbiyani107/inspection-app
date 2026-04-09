SELECT
    s.session_id,
    s.status,
    s.login_name,
    s.host_name,
    s.program_name,
    at.transaction_begin_time,
    at.transaction_state,
    st.text AS LastSQL
FROM sys.dm_exec_sessions s
INNER JOIN sys.dm_tran_session_transactions tst
    ON s.session_id = tst.session_id
INNER JOIN sys.dm_tran_active_transactions at
    ON tst.transaction_id = at.transaction_id
LEFT JOIN sys.dm_exec_connections c
    ON s.session_id = c.session_id
OUTER APPLY sys.dm_exec_sql_text(c.most_recent_sql_handle) st
WHERE s.program_name = 'Microsoft SQL Server JDBC Driver'
ORDER BY at.transaction_begin_time;
