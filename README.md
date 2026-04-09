SELECT
    s.session_id,
    at.transaction_id,
    at.transaction_begin_time,
    at.transaction_state,
    at.transaction_type
FROM sys.dm_tran_session_transactions st
INNER JOIN sys.dm_tran_active_transactions at
    ON st.transaction_id = at.transaction_id
INNER JOIN sys.dm_exec_sessions s
    ON st.session_id = s.session_id
WHERE s.session_id = 67;
