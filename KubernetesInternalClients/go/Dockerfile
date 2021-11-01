FROM golang:1.16-alpine

RUN mkdir /app

ADD . /app

WORKDIR /app

RUN go mod tidy && go build -o main .

CMD ["/app/main"]
