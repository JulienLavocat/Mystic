FROM debian:bookworm-slim

WORKDIR /app

ADD https://raw.githubusercontent.com/tilt-dev/rerun-process-wrapper/refs/heads/master/restart.sh restart.sh
ADD https://raw.githubusercontent.com/tilt-dev/rerun-process-wrapper/refs/heads/master/start.sh start.sh

RUN chmod +x start.sh && chmod +x restart.sh

COPY ./builds/server .

EXPOSE 9050

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
CMD ["./server.x86_64", "server"]