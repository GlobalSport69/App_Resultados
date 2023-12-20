#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
    CREATE DATABASE results_v2;
    \c results_v2;
    \i /docker-entrypoint-initdb.d/01_file.sql

    CREATE DATABASE hangfire;
EOSQL
