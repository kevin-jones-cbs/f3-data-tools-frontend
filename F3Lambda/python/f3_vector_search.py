import os
from sentence_transformers import SentenceTransformer, util
from momento import PreviewVectorIndexClientAsync, VectorIndexConfigurations, CredentialProvider
import openai

async def lambda_handler(event, context):
    # Read the term from the event
    term = event['term']
    print("Term: " + term)

    # Instantiate Momento Vector Client
    mvi_client = PreviewVectorIndexClientAsync(   
        configuration=VectorIndexConfigurations.Default.latest(),
        credential_provider=CredentialProvider.from_string(os.getenv("MOMENTO_API_KEY"))
    )  
    
    rsp = await mvi_client.search(   
        index_name='exicon',   
        query_vector=get_embedding("animal"),   
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

# import asyncio

# if __name__ == "__main__":
#     # Define a sample event and context
#     event = {
#         "term": "animal"  # Replace with your actual event data
#     }
#     context = {}  # Context can be an empty dictionary when testing

#     # Get an event loop and run the function
#     loop = asyncio.get_event_loop()
#     loop.run_until_complete(lambda_handler(event, context))