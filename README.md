# Cassandra Playground
A little demo app I used to learn about Cassandra

## Run
When you start the program you will be asked to pick one of the following tasks:
* Generate files (option 'g')
    * In the folder 'InputFiles' you will find 'names.json' and 'first-names.json'. This function will loop throug both these files and create a file per name including all the posible combination with first names. This means approximately 4500 files are generated each containing more than 22.000 names. 
* Fill database (option 'f')
    This function uses the files created with the function above to fill the database. If you use all the files the database will be filled with more than 100 milion entries
* Read database (option 'r')
    This function first performs a 'SELECT *' on the table (Never do this with een partition key like the one I have here!) and then loops over the outcome to query the database for each induvidual item using multiple threads

## Docker
I've used a cluster of six nodes running as docker containers. You can use the following PowerShell snippet to create your cluster. If you are running this on Windows using Linux containers like I am, than don't forget to give the Moby Linux VM in Hyper-V more RAM!

```powershell
docker run --name n1 -d -p 4000:9042 -e HEAP_NEWSIZE=256M -e MAX_HEAP_SIZE=256M -e CASSANDRA_DC=DC1 -e CASSANDRA_RACK=RA1 -e CASSANDRA_ENDPOINT_SNITCH=GossipingPropertyFileSnitch cassandra
Start-Sleep 10
docker run --name n2 --link n1:cassandra -d -p 4001:9042  -e HEAP_NEWSIZE=256M -e MAX_HEAP_SIZE=256M -e CASSANDRA_DC=DC1 -e CASSANDRA_RACK=RA2 -e CASSANDRA_ENDPOINT_SNITCH=GossipingPropertyFileSnitch cassandra
Start-Sleep 20
docker run --name n3 --link n1:cassandra -d -p 4002:9042 -e HEAP_NEWSIZE=256M -e MAX_HEAP_SIZE=256M -e CASSANDRA_DC=DC1 -e CASSANDRA_RACK=RA3 -e CASSANDRA_ENDPOINT_SNITCH=GossipingPropertyFileSnitch cassandra
Start-Sleep 30
docker run --name n4 --link n1:cassandra -d -p 4003:9042 -e HEAP_NEWSIZE=256M -e MAX_HEAP_SIZE=256M -e CASSANDRA_DC=DC2 -e CASSANDRA_RACK=RA1 -e CASSANDRA_ENDPOINT_SNITCH=GossipingPropertyFileSnitch cassandra
Start-Sleep 40
docker run --name n5 --link n1:cassandra -d -p 4004:9042 -e HEAP_NEWSIZE=256M -e MAX_HEAP_SIZE=256M -e CASSANDRA_DC=DC2 -e CASSANDRA_RACK=RA2 -e CASSANDRA_ENDPOINT_SNITCH=GossipingPropertyFileSnitch cassandra
Start-Sleep 50
docker run --name n6 --link n1:cassandra -d -p 4005:9042 -e HEAP_NEWSIZE=256M -e MAX_HEAP_SIZE=256M -e CASSANDRA_DC=DC2 -e CASSANDRA_RACK=RA3 -e CASSANDRA_ENDPOINT_SNITCH=GossipingPropertyFileSnitch cassandra
Start-Sleep 60

docker exec -it n1 nodetool status
```