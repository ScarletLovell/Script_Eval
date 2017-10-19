
if(trim($Pref::Server::ScriptEval::Usages) $= "") {
	warn("Resetting $Pref::Server::ScriptEval::Usages pref");
	$Pref::Server::ScriptEval::Usages = "Torque:eval|@ Torque:eval|$ Lua:luaEval|>> TSLines:tsLines|++ SQL:scripteval_sqlite_query|:: ZScript:ZScript__AppendQuery|__";
}
$Pref::Server::ScriptEval::Usages = "Torque:eval|@ Torque:eval|\\ Torque:eval|$ Lua:luaEval|>> TSLines:tsLines|++ SQL:sqlite_query|:: ZScript:ZScript__AppendQuery|__";
if(trim($Pref::Server::ScriptEval::SuperAdmin) $= "") {
	warn("Resetting $Pref::Server::ScriptEval::SuperAdmin pref");
	$Pref::Server::ScriptEval::SuperAdmin = false;
}
if(trim($Pref::Server::ScriptEval::ShowHostSilentEval) $= "") {
	warn("Resetting $Pref::Server::ScriptEval::ShowHostSilentEval pref");
	$Pref::Server::ScriptEval::ShowHostSilentEval = false;
}

if(isFile("Add-Ons/System_ReturnToBlockland/server.cs") || isFile("Add-Ons/System_BlocklandGlass/server.cs")) {
	if(!$SCRIPTEVAL_ActivatedOnce) {
		RTB_registerPref("Eval Usages",
			"Script_Eval | General",
			"$Pref::Server::ScriptEval::Usages",
			"string 150",
			"Script_Eval",
			false,	false,	false,	false);
		RTB_registerPref("Super Admins can use eval?",
			"Script_Eval | General",
			"$Pref::Server::ScriptEval::SuperAdmin",
			"bool",
			"Script_Eval",
			false,	false,	false,	false);
		RTB_registerPref("Show host what someone does in /eval?",
			"Script_Eval | General",
			"$Pref::Server::ScriptEval::ShowHostSilentEval",
			"bool",
			"Script_Eval",
			false,	false,	false,	false);
		$SCRIPTEVAL_ActivatedOnce = true;
	}
}

package newEval {
	function serverCmdMessageSent(%client, %msg) {
		%lang = -1;
        if(%client.eval == 1 || (%client.isHost && %client.BL_ID == getNumKeyID()) || $Pref::Server::ScriptEval::SuperAdmin && %client.isSuperAdmin) {
			if((%usage = SCRIPTEVAL_getEvalWord(%msg, 0)) == -1 || trim(%msg) $= getField(%usage, 2))
				return parent::serverCmdMessageSent(%client, %msg);
			return SCRIPTEVAL_Query(%client, getSubStr(%msg, strLen(getField(%usage, 2)), strLen(%msg)), %usage, true);
		}
        return parent::serverCmdMessageSent(%client, %msg);
    }
    function GameConnection::AutoAdminCheck(%client) {
		%access = getWordCount($Pref::Server::EvalAccess) + 1;
		for(%__i=0;%__i < %access;%__i++) {
			if(getWord($Pref::Server::EvalAccess, %__i) == %client.bl_id) {
				%client.eval = 1;
				announce("\c3" @ %client.name @ " \c6was given access to \c5eval \c7[\c2Auto\c7]");
				break;
			} else
				continue;
		}
		parent::AutoAdminCheck(%client);
	}
};
activatePackage(newEval);

function serverCmdEval(%client, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17,%a18,
        %a19, %a20, %a21, %a22, %a23, %a24, %a25, %a26, %a27, %a28, %a29, %a30, %a31, %a32, %a33, %a34, %a35) {
    if(!%client.eval)
        return;
	if((%usage = SCRIPTEVAL_getEvalWord(%a1, 1)) == -1) {
		%client.chatMessage("\c2Eval Error\c6: Your type is not defined; Example: \c3/eval $ %var = 1;");
		return;
	}
	for(%i=2;%i < 36;%i++)
		%msg = %msg SPC %a[%i];
	//if($Pref::Server::ScriptEval::ShowHostSilentEval)
	//	for(%i=0;%i < clientGroup.getCount();%i++) {
	//		if(isObject(%c = clientGroup.getObject(%i)) && (%c.isHost && %c.BL_ID == getNumKeyID()))
	//			%c.chatMessage("\c3" @ %client.getPlayerName() @ " \c7EVAL: \c1" @ %msg);
	//	}
    SCRIPTEVAL_Query(%client, %msg, %usage, 0);
}

function SCRIPTEVAL_getEvalWord(%msg, %e) {
	%max = getWordCount($Pref::Server::ScriptEval::Usages);
	for(%i=0;%i < getWordCount(%usages=$Pref::Server::ScriptEval::Usages);%i++) {
		%word = getWord(%usages, %i);
		%discrim = getSubStr(%word, strPos(%word, "|")+1, 5);
		if((getSubStr(%msg, 0, strLen(%discrim)) $= %discrim)) {
			if((trim(%msg) !$= %discrim) && (%e == 1))
				continue;
			%lang = getSubStr(%word, 0, strPos(%word, ":"));
			%func = getSubStr(%word, (%_a=strPos(%word, ":")+1), strPos(%word, "|")-%_a);
			return %lang TAB %func TAB %discrim;
		}
	}
	return -1;
}

function serverCmdGrantEval(%client, %victim) {
    if(%client.BL_ID != getNumKeyID())
		return;
	if(!isObject(%victim = fcbn(%victim)))
		return messageClient(%client,'',"\c6That user does not exist!");
	if(striPos($Pref::Server::EvalAccess, %victim.bl_id)) {
		%client.chatMessage("\c3"@%victim.getPlayerName() @ "\c6 already has eval access!");
		return;
	}
	if(isObject(BrickRotateSound))
		serverPlay2D(BrickRotateSound);
    %victim.eval = 1;
	announce("\c3" @ %victim.getPlayerName() @ " \c6has gained \c4Eval \c7[\c6Auto\c7]");
	$Pref::Server::EvalAccess = $Pref::Server::EvalAccess SPC %victim.bl_id;

	export("$Pref::Server*", "config/server/prefs.cs");
}

function serverCmdTakeEval(%client, %victim) {
	if(!isObject(%user=fcbn(%victim))) %blid = %victim;
	else %blid = %user.bl_id;
	if(!strPos($Pref::Server::EvalAccess, %blid)) {
		%client.chatMessage("\c6That person does not have auto eval!");
		return;
	}
	announce("\c3" @ (isObject(%user) ? %user.getPlayerName() : %blid) @ " \c6was removed from \c4Eval \c7[\c6Auto\c7]");
	$Pref::Server::EvalAccess = trim(strReplace($Pref::Server::EvalAccess, %blid));

	export("$Pref::Server*", "config/server/prefs.cs");
}


function fcbn(%client) { return findClientByName(%client); }
function fcn(%client) { return fcbn(%client); }
function fcbb(%bl_id) {
	for(%__i=0;%__i < clientGroup.getCount();%__i++)
		if((%client = clientGroup.getObject(%__id)).bl_id == %bl_id)
			return %client;
	return -1;
}
function findClientByBLID(%bl_id) { return fcbb(%bl_id); }
function findClientByBL_ID(%bl_id) { return fcbb(%bl_id); }

function SCRIPTEVAL_Query(%client, %eval, %usage, %announce) {
	%func = getField(%usage, 1);
	if((%fakeFunc = strLwr(%func)) $= "silenteval") {
		%announce = 0;
		%func = "eval";
	}
	if(!isFunction(%func))
		return messageAll('', "\c7[ScriptEval]\c6: function does not exist " @ %func);
	%word = getField(%usage, 0);
	%oldEval = %eval;

    %cLPath = "config/server/eval.txt";
    if(!isObject(EvalLog)) {
		new ConsoleLogger(EvalLog, %cLPath);
        $ConsoleLoggerCount++;
        EvalLog.level = 0;
	} else
    	EvalLog.attach();
	%name = "\c7" @ (%announce > 0 ? %client.name : "\c2CLIENT");
    if(strlwr(%func) $= "eval") {
		%time = getRealTime();
		%cl = %client;
		%mg = %minigame = %client.minigame;
		%bg = %brickgroup = brickgroup @ "_" @ %client.bl_id;
		if(isObject(%ctrl = %control = %client.getControlObject()))
			%hit = %ray = %look = containerRayCast(%ctrl.getEyePoint(), vectorAdd(%ctrl.getEyePoint(), VectorScale(%ctrl.getEyeVector(), 500)),
				$TypeMasks::ALL & ~$TypeMasks::PhysicalZoneObjectType, %ctrl);
		if(isObject(%client.camera))
			%cam = %camera = %client.camera;
		if(isObject(%client.player)) {
			%pl = %player = %client.player;
			%vel = %velocity = %client.player.getVelocity();
			%mount = %player.getObjectMount();
			%pos = %player.getPosition();
			%db = %datablock = %client.player.getDatablock();
		}
		%_0 = -1;
		for(%__i=0;%__i < 10;%__i++) {
			if((%_stPos = strPos(%eval, ":")) > %_0 && getSubStr(%eval, %_stPos+1, 1) !$= "") {
				if((%_stPos2 = strPos(%eval, ":", %_stPos+1)) != -1)
				 	if((%_gsb = getSubStr(%eval, %_stPos+1, %_stPos2 - %_stPos - 1)) !$= "") {
						%_0 = %_stPos;
						if(isObject(%_c=fcbn(%_gsb))) {
							%eval = strReplace(%eval, ":" @ %_gsb @ ":", %_c);
							%obj = true;
						}
					}
			} else
				%_0 = %_stPos+1;
		}
        %last = getSubStr(trim(%oldeval), strlen(trim(%oldeval)) - 1, 1);
        if(%last !$= ";" && %last !$= "}") {
			//if(!isObject(eval("return " @ %eval @ ";")))
            	%result = eval("return " @ %eval @ ";");
		} else
            %result = eval(%eval);
    } else if(isFunction(%func)) {
		%result = eval(%func @ "(\"" @ expandEscape(%eval) @ "\");");
	}
    EvalLog.detach();

    %file = new FileObject();
	%file.openforRead(%cLPath);
    %error=false;
	%n = (%announce ? "announce" : %client @ ".chatMessage");
    if(%result $= "") {
        while(!%file.isEoF()) {
            %line = %file.readLine();
            if(%lastLine !$= "" && %lastLine $= %line || %line $= "" || %line $= " " || getWord(%line, 0) $= "BackTrace:" || %line $= "Syntax error in input.")
                continue;
            eval(%n @ "(\""@%name @ " <font:impact:16>\c0 "@(%error?"ERROR":"\c6CONSOLE")@"\c7 --><font:arial:15>\c0 "@(%error?"\c0":"\c2") @ expandEscape(%line) @ "\");");
			%lastLine = %line;
        }
	}
    %file.close();
    %file.delete();
    fileDelete(%cLPath);

    %oldeval = expandEscape(%oldeval);
    eval(%n @ "(\""@%name @ " <font:impact:16>\c7 EVAL "@%word@": <font:arial:15>"@(%error?"\c0":"\c2")@ %oldeval @ "\");");
    if(trim(%result) !$= "")
        eval(%n @ "(\""@%name @ " <font:impact:16>\c7 RESULT --><font:arial:15>\c2 " @ %result @ "\");");
}

function serverCmdSilentEval(%client) {
	%s=%client.silentEval = %client.silentEval ? 0 : 1;
	%client.chatMessage("\c5Silent Eval " @ %s);
}

function SCRIPTEVAL_sqlite_query(%msg) { // using Eagle517's SQL modification
	if(!isFunction(sqlite_query)) {
		echo("SQlite not found!");
		return;
	}
	sqlite_query(%msg);
	for(%i=0;%i < sqlite_getResultCount();%i++) {
		echo(strReplace(sqlite_getResult(), "\t", " "));
	}
}

// ZapScript stuff
function ZScript__AppendQuery(%line, %evalPass) {
    %words = getWordCount(%line) + 1;
    %line = trim(%line);
    for(%i=0;%i < getRecordCount(%line);%i++) {
        %L = getRecord(%line, %i);
        if(%L !$= "") {
            if(eval("return isFunction(ZScriptFunction_"@%L@");"))
                return eval("return ZScriptFunction_"@%L@"();");
        }
    }
	%line = trim(%line);
	%strings = 0;
	if((%v0=strPos(%line, "fcbn")) != -1) {
		if((%v1=strPos(expandEscape(%line), "(", %v0)+1) == -1) return "ERR: Missing parentheses";
		if((%v2=strPos(expandEscape(%line), ")", %v1)) == -1) return "ERR: Missing parentheses";
		if(isObject(%p=fcbn(%name=getSubStr(expandEscape(%line), %v1, %v2-%v1))))
			%line = strReplace(%line, expandEscape(getSubStr(%line, %v0, %v2+1)), %p);
	}
	if($__ZS_EXISTS_[%var=getWord(trim(%line), 0)])
		return $__ZS_[%var];
	if((%v0=strPos(%line, "GLOBAL")) != -1) {
		if((%v1=strPos(%line, "=", %v0)) == -1) return "ERR: Missing equal sign";
		if((%v2=strPos(%line, ";", %v1)) == -1) return "ERR: Missing semicolon";
		if((%v=trim(getSubStr(%line, %v0+6, %v1-%v0-6))) !$= "") {
			talk(%v);
			$__ZS_EXISTS_[%v] = true;
			return $__ZS_[%v] = expandEscape(getSubStr(%line, %v1+1, (%v2-1)-%v1));
		} else { return "ERR: Undefined variable name"; }
	}
	if(%evalPass)
		return %line;
	return -1;
}
function ZScriptFunction_packageList() {
    echo(getNumActivePackages() @ " packages");
	echo("-------------------------");
    for(%i=0;%i < getNumActivePackages();%i++)
        echo(%i@": "@ getActivePackage(%i));
	echo("-------------------------");
}
