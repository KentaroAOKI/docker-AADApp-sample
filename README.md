
# Azure Active Directory Sample Application
Upload the list of groups and users registered in Azure Active Directory to Azure Storage Blob in CSV format.
## Build Docker Image
```
git clone https://github.com/KentaroAOKI/docker-AADApp-sample.git
cd docker-AADApp-sample
docker build -t aadappsample .
```
## Run the Sample Application
```
docker run -e appId="xxxxxxx-xxxxx-xxxx-xxxx-xxxxxxxxx" \
 -e scopes="https://graph.microsoft.com/.default" \
 -e tenantId="xxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxx" \
 -e clientSecret="xxxxxxxxxxxxxxxx" \
 -e storageConnectionString="DefaultEndpointsProtocol=https;AccountName=xxxxxxxx;AccountKey=xxxxxxxx;EndpointSuffix=core.windows.net" \
 -e storageContainerName="xxxxx" \
 -e storageBlobName="xxxxxx.csv" \
 aadappsample
```
