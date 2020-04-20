# MagicOnion Test  
Project to check how [MagicOnion](https://github.com/Cysharp/MagicOnion) can be used to data sharing  

## How it works  
It's Server-Client Project. Server can:  
- receive file from client  
- send file to client  
- give information about available files  
  
Client can work in two modes:  
- upload: client uploads all files from described directory to server  
- download: client downloads all files from server and saves to described folder  

## Development  
Prerequisites:  
- installed [git](https://git-scm.com/downloads)  
- installed [dotnet core](https://dotnet.microsoft.com/download)  
  
Clone repository:  
```sh
git clone https://github.com/lexao85/magiconion-test.git
```
Solution has three projects: `Server`, `Client` and `Shared`.
- `Shared` describes functions that server has to implement  
- `Server` implements server functionality: it initializes grpc server using [MagicOnion](https://github.com/Cysharp/MagicOnion) and implements functions described within `Shared`  
- `Client` implements interaction with server: it can work in download or upload mode. In download mode it downloads files from server. In upload mode it uploads files from described directory to server.  
  
`Client` and `Server` can be configured with usage of `appsettings.json` files or with environment variables or with command line arguments.  
Project was developed with [VS Code](https://code.visualstudio.com/). That's why to debug are used launch configurations from `launch.json`:  
- `Launch Server` to debug `Server`  
- `Launch Client` to debug `Client`  
  
Each project can be run with command line:  
```sh
dotnet run
```

## Deployment with Docker Compose  
Build:  
```sh
docker-compose build
```
It builds two docker images: `test-server:1.0` and `test-client:1.0`  
Start:  
```sh
docker-compose up -d
```
it start `test-server` and `test-client` containers. `test-client` starts in upload mode. It uploads files from directory `./Client/files/upload` and stopps work.  
To check that upload was successfully you can check logs:  
```sh
docker logs test-client
docker logs -f test-server
```
Or you can check list of saved files in server container:  
```sh
docker exec -it test-server /bin/ls -la /files
```
You can change mode of `test-client` by editing `docker-compose.yml`:  
```yml
      - DATASHARECLIENT_Mode=download
```
After saving changes you can recreate and start test-client again:  
```sh
docker-compose up --no-start
docker-compose start client
```
To check that download was successfully you can view logs or directory `./Client/files/download`  

## Deployment with Docker  
Build Server Docker Image:  
```sh
docker build --tag test-server:1.0 --build-arg project_name=Server .
```
Build Client Docker Image:  
```sh
docker build --tag test-client:1.0 --build-arg project_name=Client .
```
Run server docker container:
```sh
docker run --publish 12346:12346 --detach --name test-server test-server:1.0
```
Run client docker container (will work only on unix-based OS. here we need to get host docker ip (host.docker.internal didn't work) and we need to use $PWD to get current directory):
```sh
docker run --env "DATASHARECLIENT_Mode=download" --env "DATASHARECLIENT_Host=$(ip -4 addr show docker0 | grep -Po 'inet \K[\d.]+')" -v $PWD/Client/files/upload:/files/upload -v $PWD/Client/files/download:/files/download --name test-client --rm test-client:1.0
```

## Advantages and Disadvantages  
Advantages:  
- easy to develop new functionality  
- easy to deploy  
  
Disadvantages:  
- it would be better to transfer files with chunks  
- currently files are stored directly to directory. It could be problem especially if server is runned under OS Windows which has some limitations on file names. It would be better to use something like MongoDB GridFS  
- error handling should be improved  
- no tests  
