$eval = "TSLines++0 Torque@1 Torque$1 Torque+1 Torque\\1 Torque.1 Lua>>2 ZScript__3 JavaScript<<4";
	// Each word you want to use per language should look like TORQUE[%word] or LUA[%word]
package Eval {
	function serverCmdMessageSent(%client, %msg) {
		%lang = -1;
        if(%client.eval || (%client.isHost && %client.BL_ID == getNumKeyID())) {
			%words = getEvalWord(%msg);
			%lang = getWord(%words, 0);
			%w = getWord(%words, 1);
			%prop = getWord(%words, 2);
			%EvalMsg = getSubStr(%msg, strLen(%prop), strLen(%msg));
			if(%lang != -1 && %lang !$= "")
				return %client.EvalNow(%EvalMsg, %lang, 1, %w);
		}
        parent::serverCmdMessageSent(%client, %msg);
    }
    function GameConnection::AutoAdminCheck(%client) {
		%access = getWordCount($Pref::Server::EvalAccess) + 1;
		for(%i=0;%i < %access;%i++) {
			if(getWord($Pref::Server::EvalAccess, %i) == %client.bl_id) {
				%client.eval = 1;
				announce("\c3" @ %client.name @ " \c6was given access to \c5eval \c7[\c2Auto\c7]");
				break;
			} else
				continue;
		}
		parent::AutoAdminCheck(%client);
	}
};
activatePackage(Eval);

function getEvalWord(%msg) {
	%max = getWordCount($eval);
	for(%i=-1;%i < %max;%i++) {
		%word = getWord($Eval, %i);
		if(%word !$= "" && (%l = getSubStr(%word, strLen(%word)-1, 1)*1) !$= "") {
			%w = stripChars(%word, "`~!@#$%^*()_-+=1234567890,./;:'\\{[]}<>?");
			%prop = stripChars(%word, "ABCDEFGHIJKLMNOPQRSTUVWYXZabcdefghijklmnopqrstuvwyxz1234567890");
			if(getSubStr(%msg, 0, strLen(%prop)) $= %prop) {
				%lang = %l;
				return %lang SPC %w SPC %prop;
			}
		}
	}
}

function fcbn(%client) { return findClientByName(%client); }

function See(%bool, %pref) { // SHOULD MAINLY BE USED WITH EVAL
	if(%bool != true && %pref !$= "") {
		export("$Pref::" @ strReplace(%pref, "$", "") @ "*", (%path = "config/server/export.cs"));
		%file = new FileObject();
		%file.openforRead(%path);
		while(!%file.isEOF()) {
			echo("Pref of $Pref::" @ %pref @ " - " @ %file.readLine());
	    }
		%file.close();
		%file.delete();
	} else {
		dumpConsoleFunctions();
		$DumpConsoleCommands = 1;
	}
}

function serverCmdGrantEval(%client, %victim) {
    if(!%client.isHost && %client.BL_ID != getNumKeyID())
		return;
	%victim = fcbn(%victim);
	if(!isObject(%victim))
		return messageClient(%client,'',"\c6That client does not exist!");
	for(%i=0;%i <= getWordCount($Pref::Server::EvalAccess);%i++) {
		if(getWord($Pref::Server::EvalAccess, %i) == %victim.bl_id)
			return messageClient(%client, '', "\c6This person already has eval!");
		else
			continue;
	}
	if(isObject(BrickRotateSound))
		serverPlay2D(BrickRotateSound);
    %victim.eval = 1;
	announce("\c3" @ %victim.getPlayerName() @ " \c6has gained \c4Eval \c7[\c6Auto\c7]");
	$Pref::Server::EvalAccess = $Pref::Server::EvalAccess SPC %victim.bl_id;

	export("$Pref::Server*", "config/server/prefs.cs");
}

function serverCmdTakeEval(%client, %victim) {
	if(!%client.isHost && %client.BL_ID != getNumKeyID())
		return;
	if(!isObject(%victim = findClientByName(%victim)))
		return messageClient(%client,'',"\c6That client does not exist!");

	if(isObject(BrickRotateSound))
		serverPlay2D(BrickRotateSound);
	$Pref::Server::EvalAccess = strReplace($Pref::Server::EvalAccess, %victim.bl_id SPC "", "");
	%victim.eval = 0;
	announce("\c3" @ %client.getPlayerName() @ " \c6took Eval access from\c3 " @ %victim.getPlayerName() @ "\c6.");

	export("$Pref::Server*", "config/server/prefs.cs");
}

function serverCmdEval(%client, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17,%a18,
        %a19, %a20, %a21, %a22, %a23, %a24, %a25, %a26, %a27, %a28, %a29, %a30, %a31, %a32, %a33, %a34, %a35) {
    if(%client.eval < 1)
        return;
	%lang = -1;
	%words = getEvalWord(%a1);
	%lang = getWord(%words, 0);
	%w = getWord(%words, 1);
	%prop = getWord(%words, 2);
	if(%lang == -1 || %lang $= "")
		return %client.chatMessage("\c2Eval Error\c6: Your type is not defined; Example: \c3/eval $ %var = 1;");
	for(%i=2;%i < 36;%i++)
		%msg = %msg SPC %a[%i];
    %client.EvalNow(%msg, %lang, 0, %w);
}

function fcbb(%bl_id) {
	for(%i=0;%i < clientGroup.getCount();%i++)
		if((%client = clientGroup.getObject(%id)).bl_id == %bl_id)
			return %client;
	return -1;
}
function findClientByBLID(%bl_id) {
	return fcbb(%bl_id);
}

function GameConnection::EvalNow(%client, %eval, %type, %announce, %word) {
    if(%eval $= "" || !isObject(%client))
        return;
	%msg = %eval;
	%time = getRealTime();
	%cl = %client;
	%mg = %minigame = %client.minigame;
	%bg = %brickgroup = brickgroup @ "_" @ %client.bl_id;
	if(isObject(%ctrl = %control = %client.getControlObject()))
		%hit = %look = containerRayCast(%ctrl.getEyePoint(), vectorAdd(%ctrl.getEyePoint(), VectorScale(%ctrl.getEyeVector(), 500)),
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
	for(%i=0;%i < 10;%i++)
		if((%_stPos = strPos(%eval, ":")) > %_0 && getSubStr(%eval, %_stPos+1, 1) !$= "") {
			if((%_stPos2 = strPos(%eval, ":", %_stPos+1)) != -1)
			 	if((%_gsb = getSubStr(%eval, %_stPos+1, %_stPos2 - %_stPos - 1)) !$= "") {
					%_0 = %_stPos;
					if(isObject(fcbn(%_gsb)))
						%eval = strReplace(%eval, ":" @ %_gsb @ ":", fcbn(%_gsb));
				}
		}
		else {
			%_0 = %stPos+1;
		}
    %path = "config/server/eval.txt";
    if(!isObject(EvalLog)) {
		new ConsoleLogger(EvalLog, %path);
        $ConsoleLoggerCount++;
        EvalLog.level = 0;
	} else
    	EvalLog.attach();
	if(%type == 0) {
		if(%announce > 0) {
			if(%eval $= "end") {
				%err=1;
				%m = "\c7" @ %client.name @ "\c6: <font:impact:16>\c7 RESULT --><font:arial:15>\c2 " @ (%result = eval(%client.evalMsg SPC "%err=0;"));
				if(%result !$= "")
					announce(%m);
				%client.evalMsg = "";
			} else {
				%client.evalMsg = %client.evalMsg NL %eval;
				announce("\c7" @ %client.name @ "\c6: " @ " <font:impact:16>\c7 LINES --><font:arial:15>\c2 " @ %eval);
			}
		}
	}
	if(%type == 1) {
		%last = getSubStr(trim(%eval), strlen(trim(%eval)) - 1, 1);
		if(%last !$= ";" && %last !$= "}") {
			if(%announce > 0) {
				%m = "\c7" @ (%announce > 0 ? %client.name : "\c2CLIENT") @ "\c6: <font:impact:16>\c7 RESULT --><font:arial:15>\c2 " @ (%result = eval("return " @ %eval @ ";"));
				%_doNotContinue=true;
				if(%result !$= "")
        			announce(%m);
			}
			else
				%client.chatMessage(%m);
		} else if(!%_doNotContinue)
			eval(%eval @ " %err=0;");
	}
    if(%type == 2 && isFunction(luaEval))
    	luaEval(%eval @ " return 0");
	else if(%type == 3 && isFunction(ZScript__AppendQuery))
		ZScript__AppendQuery("\"" @ %eval @ "\"");
	else if(%type == 4 && isFunction(JS_Eval))
		JS_Eval(%eval);
    EvalLog.detach();

	%time = getRealTime() - %time;
	if(%time < 0 )
		%time = 0;

	$evalLastLine = "";
    for(%i=0;%i < 20;%i++)
        %sp = %sp @ " ";
	%file = new FileObject();
	%file.openforRead(%path);
	if(%result $= "")
		while(!%file.isEOF()) {
			%line = %file.readLine();
			if($DumpConsoleCommands > 0 && getWord(%line, 3) !$= "virtual" || %line $= "undefined" && %type == 4)
				continue;
			%linesRead++;
	        if(%line $= "" || %line $= " " || %lastLine $= %line || getWord(%line, 0) $= "BackTrace:" || %line $= "Syntax error in input.")
	            continue;
			if(getWord(%line, 0) $= "<input>" || getWord(%line, 0) @ " " @ getWord(%line, 1) $= "eval error")
				%err = 0;
	        %lastLine = %line;
			if(%announce < 1)
	        	messageClient(%client, '', "\c2Client\c6:" @ %sp @ "\c6- " @ " <font:impact:16>\c7 EVAL --><font:arial:15>\c2 " @ %line);
			else
				announce("\c7" @ %client.name @ "\c6: " @ " <font:impact:16>\c7 EVAL --><font:arial:15>\c2 " @ %line);
		}
	%file.close();
	%file.delete();
	$DumpConsoleCommands = 0;
    fileDelete(%path);
	if(%announce > 0)
		announce("\c7" @ %client.name @ " <color:5c5c3d>" @ %word @ " \c7[\c3" @ %time @ "\c7]\c6:" @ (%err < 1 ? "\c6 " : "\c0 ") @ %msg);
	else
		messageClient(%client, '', "\c7" @ "\c2Client <color:5c5c3d>" @ %word SPC "\c7[\c3" @ %time @ "\c7]\c6:" @ (%err < 1 || %result $= "" ? "\c6" : "\c0") @ %msg);
}
