FROM mcr.microsoft.com/devcontainers/dotnet:11.0-noble

USER root

# Installa strumenti utili per lo sviluppo
RUN apt-get update && apt-get install -y \
    git \
    curl \
    vim \
    nano \
    htop \
    tree \
    jq \
    ca-certificates \
    gnupg \
    iproute2 \
    iputils-ping \
    dnsutils \
    netcat-openbsd \
    openssl \
    procps \
    lsof \
    mariadb-client \
    redis-tools \
    httpie \
    yq \
    && rm -rf /var/lib/apt/lists/*

# Configura Git (opzionale)
RUN git config --system init.defaultBranch main
RUN git config --system --add safe.directory "/workspaces/*"

# Installa global tools .NET per l'utente con cui lavorerai nel Dev Container
USER vscode
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/home/vscode/.dotnet/tools"
