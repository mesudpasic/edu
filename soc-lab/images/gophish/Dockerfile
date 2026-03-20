FROM debian:bookworm-slim

ARG GOPHISH_VERSION=0.12.1
RUN apt-get update && apt-get install -y --no-install-recommends ca-certificates wget unzip \
    && rm -rf /var/lib/apt/lists/*

RUN wget -q "https://github.com/gophish/gophish/releases/download/v${GOPHISH_VERSION}/gophish-v${GOPHISH_VERSION}-linux-64bit.zip" -O /tmp/gophish.zip \
    && unzip -q /tmp/gophish.zip -d /opt/gophish \
    && rm /tmp/gophish.zip \
    && chmod +x /opt/gophish/gophish

WORKDIR /opt/gophish
# Admin UI must listen on all interfaces inside the container; TLS disabled for simpler lab HTTPS offloading.
RUN sed -i 's/"listen_url": "127.0.0.1:3333"/"listen_url": "0.0.0.0:3333"/' config.json \
    && sed -i 's/"use_tls": true/"use_tls": false/' config.json

EXPOSE 3333 80
CMD ["./gophish"]
