sudo docker network create --attachable -d bridge smnetwork
sudo docker network ls

sudo docker run -it -d --name mongo-container -p 27017:27017 --network smnetwork --restart=always -v mongodb_data_container:/data/db mongo:latest

# I was not able to pull image on 1/7/2023
# https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-ver16&pivots=cs1-bash
docker run -d --name sql-container --network smnetwork --restart always -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=$tr0ngS@P@ssw0rd02' -e 'MSSQL_PID=Enterprise' -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest

docker run --name sql-container --network smnetwork\
-e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=$tr0ngS@P@ssw0rd02' \
-e 'MSSQL_PID=Enterprise' -p 1433:1433 \
-d mcr.microsoft.com/mssql/server:2022-latest