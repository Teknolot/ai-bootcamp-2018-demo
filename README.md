# Demo app used during AI Bootcamp Istanbul "Rapid AI in Mobile Experiences" session.

POC [Azure Functions](https://azure.microsoft.com/en-us/services/functions/) code running [Cognitive Services Face APIs](https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f3039523a) to train and match faces to individuals.

### API Schema

**Endpoint** : api/AddFace   
**Method** : POST

Adding a new person with a face image.

Sample Request
```
{   
  "imageUrl":"https://i1.rgstatic.net/ii/profile.image/644263821971457-1530615873370_Q512/Cihan_Yakar.jpg",   
  "personName":"Daron"   
}   
```
Sample Response
```
{
    "faceId": "6d3e9486-ac88-42ac-9ec8-714b37dab654",
    "personId": "2c9e53a3-bbe3-43a0-9c28-0ec349e0ea62"
}
```

Adding additional faces to a person identified by its `personId`.

Sample Request
```
{   
  "imageUrl":"https://i1.rgstatic.net/ii/profile.image/644263821971457-1530615873370_Q512/Cihan_Yakar.jpg",   
  "personId":"2c9e53a3-bbe3-43a0-9c28-0ec349e0ea62"   
}   
```
Sample Response
```
{
    "faceId": "6d3e9486-ac88-42ac-9ec8-714b37dab654",
    "personId": "2c9e53a3-bbe3-43a0-9c28-0ec349e0ea62"
}
```

**Endpoint** : api/ValidateFace   
**Method** : POST

Finding who a particular image belongs to if any.

Sample Request
```
{
    "imageUrl":"https://avatars2.githubusercontent.com/u/11349626?v=4"
} 
```
Sample Response
```
{
    "name": "Yigit",
    "personId": "1c316eff-61f1-4c7f-be9d-8d8c9dba53dd"
}
```

**Endpoint** : api/UploadFile   
**Method** : POST

Uploads files to a storage account and returns its URL to be used in our APIs. 

Multipart file upload is expected as part of the form post data in the request.

Sample Response
```
["https://facevalidationfunctions.blob.core.windows.net/tempphotos/teknolot.png"]
```
