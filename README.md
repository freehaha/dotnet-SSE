# To Run

`dotnet run`


# Testing

## Subscribe to notification

```sh
	curl --http1.1 -k -N -XGET https://localhost:5001/notification
```

## Sending notification

```sh
	curl -XPOST https://localhost:5001/notification -v -k 
```
