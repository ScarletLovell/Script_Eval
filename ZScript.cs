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
    echo(getNumActivePackages() @ " active packages");
	echo("-------------------------");
    for(%i=0;%i < getNumActivePackages();%i++)
        echo(%i@": "@ getActivePackage(%i));
	echo("-------------------------");
}
