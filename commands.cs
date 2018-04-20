function serverCmdSilentEval(%client) {
	%client.silentEval = (%client.silentEval ? 0 : 1);
	%client.chatMessage("\c2CLIENT \c6Silent Eval " @ (%client.silentEval ? "enabled" : "\c0disabled"));
}

function serverCmdEval(%client, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17,%a18,
        %a19, %a20, %a21, %a22, %a23, %a24, %a25, %a26, %a27, %a28, %a29, %a30, %a31, %a32, %a33, %a34, %a35) {
    if(!%client.eval)
        return;
	if((%usage = SCRIPTEVAL_getEvalWord(%client, %a1, 1)) == -1) {
		%client.chatMessage("\c2Eval Error\c6: Your type is not defined; Example: \c3/eval $ \%var = 1;");
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
	$Pref::Server::EvalAccess = trim(strReplace($Pref::Server::EvalAccess, %blid, ""));

	export("$Pref::Server*", "config/server/prefs.cs");
}

function serverCmdEvalCommands(%client)
{
	%client.chatMessage("\c6Eval Commands:");
	%client.chatMessage("\c2/eval [discriminator] [line]");
	%client.chatMessage("\c2/grantEval [name or BL_ID]");
	%client.chatMessage("\c2/takeEval [name or BL_ID]");
	%client.chatMessage("\c2/silentEval");
}

function serverCmdEvalCmds()
{
	serverCmdEvalCommands();
}

function serverCmdEvalHelp()
{
	serverCmdEvalCommands();
}
