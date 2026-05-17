CREATE TABLE IF NOT EXISTS logs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    timestamp timestamptz NOT NULL DEFAULT now(),
    level varchar(10) NOT NULL,
    source varchar(100) NOT NULL,
    message text NOT NULL,
    http_method varchar(10),
    http_path varchar(500),
    http_status int,
    duration_ms double precision,
    trace_id varchar(50),
    exception_type varchar(200),
    exception_message text
);
