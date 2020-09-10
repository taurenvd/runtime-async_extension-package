##Usage
public async Response GetResponse()
{
	var response = await UnityWebRequest("https://somedata.com/file.123"); //wait download from web resource

	await Timer(3, null); // wait some timers/coroutines

	await WaitUntil(()=>_some_int_field == 5); // wait CustomYieldInstructions

	return response;
}