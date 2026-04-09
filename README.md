SELECT
    r.session_id,
    r.blocking_session_id,

    s.status AS session_status,
    r.status AS request_status,

    r.command,
    r.wait_type,
    r.wait_time,
    r.last_wait_type,
    r.cpu_time,
    r.total_elapsed_time,

    DB_NAME(r.database_id) AS DatabaseName,

    s.login_name,
    s.host_name,
    s.program_name,

    -- transaction info
    at.transaction_begin_time,
    at.transaction_state,

    -- SQL text
    st.text AS RunningSQL

FROM sys.dm_exec_requests r

INNER JOIN sys.dm_exec_sessions s
    ON r.session_id = s.session_id

LEFT JOIN sys.dm_tran_session_transactions tst
    ON r.session_id = tst.session_id

LEFT JOIN sys.dm_tran_active_transactions at
    ON tst.transaction_id = at.transaction_id

OUTER APPLY sys.dm_exec_sql_text(r.sql_handle) st

WHERE
    r.blocking_session_id <> 0
    OR r.session_id IN (
        SELECT blocking_session_id
        FROM sys.dm_exec_requests
        WHERE blocking_session_id <> 0
    )

ORDER BY
    r.blocking_session_id DESC,
    r.session_id;
