FROM barichello/godot-ci:mono-4.3 AS build

WORKDIR /app

RUN wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt update && \
    apt-get install -y dotnet-sdk-8.0

COPY . .

RUN mkdir -p ./builds/server && godot --headless --verbose --export-debug "Server Debug" ./builds/server/server.x86_64

FROM debian:bookworm-slim

WORKDIR /app

COPY --from=build /app/builds/server .

RUN ls -la

EXPOSE 9050

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
CMD ["./server.x86_64", "server"]
