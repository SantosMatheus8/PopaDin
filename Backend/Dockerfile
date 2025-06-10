FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

# Instalar curl antes de baixar wait-for-it.sh
RUN apt-get update && apt-get install -y curl

# Baixar e configurar wait-for-it.sh
RUN curl -o /usr/local/bin/wait-for-it.sh https://raw.githubusercontent.com/vishnubob/wait-for-it/master/wait-for-it.sh && \
    chmod +x /usr/local/bin/wait-for-it.sh

# Instalar mssql-tools e unixodbc-dev com aceitação automática da licença
RUN ACCEPT_EULA=Y DEBIAN_FRONTEND=noninteractive apt-get install -y mssql-tools unixodbc-dev

ENV PATH="$PATH:/opt/mssql-tools/bin"

USER mssql