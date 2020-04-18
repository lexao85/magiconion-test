FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS sdk
ARG project_name=Server
RUN echo Building $project_name
COPY . ./workspace

RUN dotnet publish ./workspace/$project_name/$project_name.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
ARG project_name=Server
ENV env_project_name=$project_name
RUN echo Deploying $project_name
COPY --from=sdk /app .
ENTRYPOINT dotnet $env_project_name.dll