// ---
// -- Gets the discrim, func, and lang name from $Pref::Server::ScriptEval::Usages -- //
// ---
function SCRIPTEVAL_getEvalWord(%client, %msg, %e) {
	%max = getWordCount($Pref::Server::ScriptEval::Usages);
	for(%i=0;%i < getWordCount(%usages=$Pref::Server::ScriptEval::Usages);%i++) {
		%word = getWord(%usages, %i);
		%discrim = getSubStr(%word, strPos(%word, "|")+1, 5);
		if((getSubStr(%msg, 0, strLen(%discrim)) $= %discrim)) {
			if((trim(%msg) !$= %discrim) && (%e == 1))
				continue;
			%lang = getSubStr(%word, 0, strPos(%word, ":"));
			%func = getSubStr(%word, (%_a=strPos(%word, ":")+1), strPos(%word, "|")-%_a);
			if(strLwr(%func) $= "eval" && %client.silentEval)
				%func = "silenteval";
			return %lang TAB %func TAB %discrim;
		}
	}
	return -1;
}

// ---
// -- I created this as-well if you don't have my edited version, so it's more lightweight with script_eval -- //
// -- add this as a function to $Pref::Server::ScriptEval::Usages for proper usage in script_eval -- //
// ---
function SCRIPTEVAL_sqlite_query(%msg) {
	if(!isFunction(sqlite_query)) {
		echo("SQlite not found!");
		return;
	}
	sqlite_query(%msg);
	for(%i=0;%i < sqlite_getResultCount();%i++) {
		echo(strReplace(sqlite_getResult(), "\t", " "));
	}
}

// ---
// -- All the requested functions that people wanted added -- //
// ---
function fcbn(%client) { return findClientByName(%client); }
function fcn(%client) { return findClientByName(%client); }
function fcbb(%bl_id) {
	for(%__i=0;%__i < clientGroup.getCount();%__i++)
		if((%client = clientGroup.getObject(%__id)).bl_id == %bl_id || %client.bl_id $= %bl_id)
			return %client;
	return -1;
}
function findClientByBLID(%bl_id) { return fcbb(%bl_id); }
function findClientByBL_ID(%bl_id) { return fcbb(%bl_id); }
