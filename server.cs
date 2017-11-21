

if(isFile("Add-Ons/System_ReturnToBlockland/server.cs") || isFile("Add-Ons/System_BlocklandGlass/server.cs")) {
	if(trim($Pref::Server::ScriptEval::Usages) $= "") {
		warn("Resetting $Pref::Server::ScriptEval::Usages pref");
		$Pref::Server::ScriptEval::Usages = "Torque:eval|@ Torque:eval|$ Lua:luaEval|>> TSLines:tsLines|++ SQL:scripteval_sqlite_query|:: ZScript:ZScript__AppendQuery|__";
	}
	if(trim($Pref::Server::ScriptEval::SuperAdmin) $= "") {
		warn("Resetting $Pref::Server::ScriptEval::SuperAdmin pref");
		$Pref::Server::ScriptEval::SuperAdmin = false;
	}
	if(trim($Pref::Server::ScriptEval::ShowHostSilentEval) $= "") {
		warn("Resetting $Pref::Server::ScriptEval::ShowHostSilentEval pref");
		$Pref::Server::ScriptEval::ShowHostSilentEval = false;
	}
	if(trim($Pref::Server::ScriptEval::ShowResults) $= "") {
		warn("Resetting $Pref::Server::ScriptEval::ShowResults pref");
		$Pref::Server::ScriptEval::ShowResults = true;
	}

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
		RTB_registerPref("Show results to everyone in chat eval?",
			"Script_Eval | General",
			"$Pref::Server::ScriptEval::ShowResults",
			"bool",
			"Script_Eval",
			false,	false,	false,	false);

		if(isFile("./functions.cs"))
			exec("./functions.cs");
		if(isFile("./commands.cs"))
			exec("./commands.cs");
		if(isFile("./ZScript.cs"))
			exec("./ZScript.cs");

		$SCRIPTEVAL_ActivatedOnce = true;
	}
}

package newEval {
	function serverCmdMessageSent(%client, %msg) {
		%lang = -1;
        if(%client.eval == 1 || (%client.isHost && %client.BL_ID == getNumKeyID()) || $Pref::Server::ScriptEval::SuperAdmin && %client.isSuperAdmin) {
			if((%usage = SCRIPTEVAL_getEvalWord(%client, %msg, 0)) == -1 || trim(%msg) $= getField(%usage, 2))
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

function SCRIPTEVAL_Query(%client, %eval, %usage, %announce) {
	%func = getField(%usage, 1);
	%lwrFunc = strLwr(%func);
	if((%fakeFunc = %func) $= "silenteval") {
		%announce = 0;
		%func = "eval";
	}
	if(!isFunction(%func) && %lwrFunc !$= "tslines")
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
    if(%lwrFunc $= "eval") {
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
	} else if(%lwrFunc $= "tslines") {
		%lwrEval = strLwr(%eval);
		%n = (%announce ? "announce" : %client @ ".chatMessage");
		if(%lwrEval $= "end") {
			if(trim(%client.tsLines) $= "") {
				eval(%n @ "(\""@%name @ " <font:impact:16>\c7 RESULT --><font:arial:15>\c2 No TSLines to evaluate\");");
			} else {
				%result = eval(%client.tsLines);
				%client.tsLines = "";
			}
		} else if(%lwrEval $= "cancel") {
			eval(%n @ "(\""@%name @ " <font:impact:16>\c7 RESULT --><font:arial:15>\c2 TSLines canceled\");");
			%client.tsLines = "";
		} else if(getWord(%lwrEval, 0) $= "edit") {
			if(stripChars((%num=getWord(%eval, 1)), "123456789") !$= "")
				eval(%n @ "(\""@%name @ " <font:impact:16>\c7 RESULT --><font:arial:15>\c2 Found no numbers (edit #)\");");
			else if((%field=trim(getField(%client.tsLines, %num))) $= "") {
				talk(%num);
				eval(%n @ "(\""@%name @ " <font:impact:16>\c7 RESULT --><font:arial:15>\c2 That TSLine does not exist, check view\");");
			} else {
				%words = getWords(%eval, 2, 500);
				%client.tsLines = setField(%client.tsLines, %num, %words);
				eval(%n @ "(\""@%name @ " <font:impact:16>\c7 RESULT --><font:arial:15>\c2 Changed \c6field " @ %num @ " \c2to\c6 " @ %words @ "\");");
			}
		} else if(%lwrEval $= "view") {
			if(trim(strLen(%client.tsLines) > 0))
				for(%i=1;%i < getFieldCount(%client.tsLines);%i++)
					eval(%n @ "(\""@%name @ " <font:impact:16>\c7 RESULT --><font:arial:15> \c6" @ %i @": \c2"@ strReplace(getField(%client.tsLines, %i), "\"", "\\\"") @ "\");");
			else
				eval(%n @ "(\""@%name @ " <font:impact:16>\c7 RESULT --><font:arial:15> \c2 No TSLines to view\");");
		} else {
			%client.tsLines = %client.tsLines TAB %eval;
			eval(%n @ "(\""@%name @ " <font:impact:16>\c7 RESULT --><font:arial:15>\c2 added \c6" @ expandEscape(%eval) @ "\c2 to TSLines" @ "\");");
		}
    } else if(isFunction(%func)) {
		%result = eval("return " @ %func @ "(\"" @ expandEscape(%eval) @ "\");");
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
            eval((%r==1 ?%n:%client@".chatMessage") @ "(\""@(%r?%name:"\c2CLIENT")@ " <font:impact:16>\c0 "@(%error?"ERROR":"\c6CONSOLE")@"\c7 --><font:arial:15>\c0 "@(%error?"\c0":"\c2") @ expandEscape(%line) @ "\");");
			%lastLine = %line;
        }
	}
    %file.close();
    %file.delete();
    fileDelete(%cLPath);

	%r = $Pref::Server::ScriptEval::ShowResults;
    %oldeval = expandEscape(%oldeval);
    eval(%n @ "(\""@%name @ " <font:impact:16>\c7 EVAL "@%word@": <font:arial:15>"@(%error?"\c0":"\c2")@ %oldeval @ "\");");
    if(trim(%result) !$= "")
        eval((%r==1 ?%n:%client@".chatMessage") @ "(\""@(%r?%name:"\c2CLIENT")@ " <font:impact:16>\c7 RESULT --><font:arial:15>\c2 " @ %result @ "\");");
}
