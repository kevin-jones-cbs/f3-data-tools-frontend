import os 
from momento import PreviewVectorIndexClientAsync, VectorIndexConfigurations, CredentialProvider
import openai
import asyncio
import json

async def async_handler(event, context):
    print (event)
    body = event['body']
    term = json.loads(body)['term']

    # Instantiate Momento Vector Client
    mvi_client = PreviewVectorIndexClientAsync(   
        configuration=VectorIndexConfigurations.Default.latest(),
        credential_provider=CredentialProvider.from_string(os.getenv("MOMENTO_API_KEY"))
    )  
    
    rsp = await mvi_client.search(   
        index_name='exicon',   
        query_vector=get_embedding(term),   
        top_k=50,
        metadata_fields=['Id']
    )

    results = []
    for result in rsp.hits:   
        print(f"{result.metadata['Id']}: {round(result.score, 3)}")
        results.append({
            'Id': result.metadata['Id'],
            'Score': round(result.score, 3)
        })
        
    return {
        'statusCode': 200,
        'body': results
    }

# def get_embedding(i):	
# 	model = SentenceTransformer("all-MiniLM-L6-v2")	
# 	model.max_seq_length = 256
# 	return model.encode(i, normalize_embeddings=True)

def get_embedding(text, engine="text-embedding-ada-002"):
    openai.api_key = os.getenv("OPEN_AI_EMBED_TOKEN")

    # Generate the embedding
    response = openai.embeddings.create(input=text, model=engine)
    embedding = response.data[0].embedding

    return embedding

def lambda_handler(event, context):
    loop = asyncio.get_event_loop()
    return loop.run_until_complete(async_handler(event, context))

# import asyncio

# if __name__ == "__main__":
#     # Define a sample event and context
#     event = {'version': '2.0', 'routeKey': '$default', 'rawPath': '/', 'rawQueryString': '', 'headers': {'content-length': '26', 'x-amzn-tls-version': 'TLSv1.2', 'x-forwarded-proto': 'https', 'postman-token': '79d096a6-31e7-44d2-9933-4f5c16d596b9', 'x-forwarded-port': '443', 'x-forwarded-for': '73.151.4.230', 'accept': '*/*', 'x-amzn-tls-cipher-suite': 'ECDHE-RSA-AES128-GCM-SHA256', 'x-amzn-trace-id': 'Root=1-655d8d10-60dfe55d02ee04dc1bef7b12', 'host': 'wdd5t63r4ve5btbdv5yypcnugq0fgeow.lambda-url.us-west-1.on.aws', 'content-type': 'application/json', 'accept-encoding': 'gzip, deflate, br', 'user-agent': 'PostmanRuntime/7.35.0'}, 'requestContext': {'accountId': 'anonymous', 'apiId': 'wdd5t63r4ve5btbdv5yypcnugq0fgeow', 'domainName': 'wdd5t63r4ve5btbdv5yypcnugq0fgeow.lambda-url.us-west-1.on.aws', 'domainPrefix': 'wdd5t63r4ve5btbdv5yypcnugq0fgeow', 'http': {'method': 'GET', 'path': '/', 'protocol': 'HTTP/1.1', 'sourceIp': '73.151.4.230', 'userAgent': 'PostmanRuntime/7.35.0'}, 'requestId': '45924bba-8cc8-4646-b5a0-23301f668e04', 'routeKey': '$default', 'stage': '$default', 'time': '22/Nov/2023:05:09:36 +0000', 'timeEpoch': 1700629776665}, 'body': '{\r\n    "term": "animal"\r\n}', 'isBase64Encoded': False}

#     context = {}  # Context can be an empty dictionary when testing

#     # Get an event loop and run the function
#     loop = asyncio.get_event_loop()
#     loop.run_until_complete(lambda_handler(event, context))