FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine-amd64

EXPOSE 8080

RUN mkdir /app
WORKDIR /app
COPY ./linux64_musl/. ./

RUN chmod +x ./lite-social-network-backend
CMD ["./lite-social-network-backend", "--urls", "http://0.0.0.0:8080"]