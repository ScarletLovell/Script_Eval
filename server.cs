$Eval = "@ $ + \\";
package Eval {
	function serverCmdMessageSent(%client, %msg) {
        if(%client.eval || (%client.isHost && %client.BL_ID == getNumKeyID())) {
            for(%i=0;%i <= getWordCount($eval);%i++)
                if(getSubStr(%msg, 0, strLen(getWord($eval, %i))) $= getWord($eval, %i))
                    return %client.EvalNow(getSubStr(%msg, strLen(getWord($eval, %i)), strLen(%msg)), 0, 1);
            for(%i=0;%i <= getWordCount($luaEval);%i++)
                if(getSubStr(%msg, 0, 2) $= ">>")
                    return %client.EvalNow(getSubStr(%msg, 2, strLen(%msg)), 2, 1);
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
	serverPlay2D(770);
    %victim.eval = 1;
	announce("\c3" @ %victim.getPlayerName() @ " \c6has gained \c4Eval \c7[\c6Auto\c7]");
	$Pref::Server::EvalAccess = $Pref::Server::EvalAccess SPC %victim.bl_id;

	export("$Pref::Server*", "config/server/prefs.cs");
}

function serverCmdTakeEval(%client, %victim) {
	if(!%client.isHost && %client.BL_ID != getNumKeyID())
		return;
	%this = isObject(%victim) && %victim;
	if(!isObject(%victim))
		return messageClient(%client,'',"\c6That client does not exist!");
	if(removeWord(%))

	serverPlay2D(BrickRotateSound);
	$Pref::Server::EvalAccess = strReplace($Pref::Server::EvalAccess, %victim.bl_id, "");
	%victim.eval = 0;
	announce("\c3" @ %client.getPlayerName() @ " \c6took Eval access from\c3 " @ %victim.getPlayerName() @ "\c6.");

	export("$Pref::Server*", "config/server/prefs.cs");
}

function serverCmdEval(%client, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17,%a18,
        %a19, %a20, %a21, %a22, %a23, %a24, %a25, %a26, %a27, %a28, %a29, %a30, %a31, %a32, %a33, %a34, %a35) {
    if(%client.eval < 1)
        return;
	for(%i=1;%i < 36;%i++)
		%msg = %msg SPC %a[%i];
    %client.EvalNow(getSubStr(%msg, 0, strLen(%msg)), 0, 0);
}

function GameConnection::EvalNow(%client, %eval, %type, %announce) {
    if(%eval $= "" || !isObject(%client))
        return;
	%time = getRealTime();
	%cl = %client;
	%mg = %minigame = %client.minigame;
	%bg = %brickgroup = brickgroup @ "_" @ %client.bl_id;
	%ctrl = %control = %client.getControlObject();
	%hit = %look = containerRayCast(%ctrl.getEyePoint(), vectorAdd(%ctrl.getEyePoint(), VectorScale(%ctrl.getEyeVector(), 100)), $TypeMasks::ALL, %ctrl);
	if(isObject(%client.camera))
		%cam = %camera = %client.camera;
	if(isObject(%client.player)) {
		%pl = %player = %client.player;
		%mount = %player.getObjectMount();
		%pos = %player.getPosition();
		%db = %datablock = %client.player.getDatablock();
	}
    %path = "config/server/eval.txt";
    if(!isObject(EvalLog)) {
		new ConsoleLogger(EvalLog, %path);
        $ConsoleLoggerCount++;
        EvalLog.level = 0;
	} else
    	EvalLog.attach();
	if(%type < 2) {
		%last = getSubStr(trim(%eval), strlen(trim(%eval)) - 1, 1);
		if(%last !$= ";" && %last !$= "}") {
			if(%announce > 0)
        		announce("\c7" @ %client.name @ "\c6:  " @ "  <font:impact:16>\c7 RESULT --><font:arial:15>\c2 " @ (%result = eval("return " @ %eval @ ";")));
		} else
			%err = eval(%eval @ " %err=0;");
	}
    if(%type == 2) {
		if(isFunction(luaEval))
        	%err = luaEval(%eval @ " return 0");
		else
			warn("LUA is not enabled!");
	}
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
			%line = strReplace(%file.readLine();
			if($DumpConsoleCommands > 0 && getWord(%line, 3) !$= "virtual")
				continue;
			%linesRead++;
	        if(%line $= "" || %line $= " " || %client.evalLastLine $= %line || getWord(%line, 0) $= "BackTrace:" || %line $= "Syntax error in input.")
	            continue;
			if(getWord(%line, 0) $= "<input>" || getWord(%line, 0) @ " " @ getWord(%line, 1) $= "eval error")
				%err = 0;
	        %client.evalLastLine = %line;
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
		announce("\c7" @ %client.name @ " <color:5c5c3d>" @ (%type < 2 ? "Torque" : "\c5LUA") @ " \c7[\c3" @ %time @ "\c7]\c6:" @ (%err < 1 ? "\c6 " : "\c0 ") @ %eval);
	else
		messageClient(%client, '', "\c7" @ "\c2Client <color:5c5c3d>" @ (%type < 2 ? "Torque" : "\c5LUA") @ "\c7[\c3" @ %time @ "\c7]\c6:" @ (%err < 1 || %result $= "" ? "\c6 " : "\c0 ") @ %eval);
}
