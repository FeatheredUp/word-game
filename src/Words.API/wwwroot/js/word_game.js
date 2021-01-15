//JSLint directive
/*global window */

"use strict";

// https://developer.mozilla.org/en-US/docs/Web/API/WindowEventHandlers/onbeforeunload
window.addEventListener('beforeunload', function (e) {
    e.preventDefault();
    e.returnValue = '';
});

window.word_game = {};

window.word_game.WindowEventManager = function () {

    function setOnLoad(callback) {
        if (typeof(callback) === "function") {
            window.onload = function () {
                callback();
            };
        }
    }

    function setOnResize(callback) {
        if (typeof(callback) === "function") {
            window.onresize = function () {
                callback();
            };
        }
    }

    return {
        setOnLoad: setOnLoad,
        setOnResize: setOnResize
    };
};

window.word_game.ButtonControlEventManager = function (buttonControl) {

    function setOnClick(callback) {
        if (typeof(callback) === "function") {
            buttonControl.onclick = function () {
                callback();
                buttonControl.blur();
            };
        }
    }

    return {
        setOnClick: setOnClick
    };
};


window.word_game.SquareControlEventManager = function (squareControl, lookupKey, findByLookupKey) {
    var onDrop = null, draggable = false, droppable = false;

    squareControl.ondragstart = function (evt) {
        evt.dataTransfer.clearData("text");
        if (draggable) {
            evt.dataTransfer.setData("text", lookupKey);
        }
    };

    squareControl.ondragover = function (evt) {
        if (droppable) {
            evt.preventDefault();
        }
    };

    squareControl.ondrop = function (evt) {
        if (droppable) {
            evt.preventDefault();
            var otherLookupKey = evt.dataTransfer.getData("text");
            if (otherLookupKey !== lookupKey) {
                if (typeof(onDrop) === "function") {
                    onDrop(findByLookupKey(lookupKey), findByLookupKey(otherLookupKey));
                }
            }
        }
        evt.dataTransfer.clearData("text");
    };

    function setOnDrop(callback) {
        if (typeof(callback) === "function") {
            onDrop = callback;
        }
    }

    function setDraggable(flag) {
        if (typeof(flag) === "boolean") {
            draggable = flag;
            squareControl.draggable = flag;
        }
    }

    function setDroppable(flag) {
        if (typeof(flag) === "boolean") {
            droppable = flag;
        }
    }

    return {
        setOnDrop: setOnDrop,
        setDraggable: setDraggable,
        setDroppable: setDroppable
    };
};

window.word_game.SquareManager = function (squareControl, createLookupKey, findByLookupKey, originalSquareClassName) {
    var squareEventManager = null,
        splitId = squareControl.id.split("_"),
        squareType = splitId[0],
        rowNumber = parseInt(splitId[1], 10),
        columnNumber = 0,
        lookupKey = null,
        valueControls = squareControl.getElementsByClassName("squarevalue"),
        heightControls = squareControl.getElementsByClassName("squareheight"),
        valueControl = (valueControls.length > 0
            ? valueControls[0]
            : null),
        heightControl = (heightControls.length > 0
            ? heightControls[0]
            : null),
        currentLetterValue = null,
        currentHeightValue = null,
        originalLetterValue = null,
        originalHeightValue = null,
        clear = false,
        locked = false,
        disabled = false,
        startup = false,
        atMaxHeight = false;

    function hasOriginalValue() {
        return (originalLetterValue !== null || originalHeightValue !== null);
    }

    function isRackSquare() {
        return squareType === "rack";
    }

    function isBoardSquare() {
        return squareType === "board";
    }

    if (isBoardSquare()) {
        columnNumber = parseInt(splitId[2], 10);
    }

    lookupKey = createLookupKey(rowNumber, columnNumber);
    squareEventManager = new window.word_game.SquareControlEventManager(squareControl, lookupKey, findByLookupKey);

    function getRowNumber() {
        return rowNumber;
    }

    function getColumnNumber() {
        return columnNumber;
    }

    function getLookupKey() {
        return lookupKey;
    }

    function getSquareEventManager() {
        return squareEventManager;
    }

    function resizeSquare(squareHeight, letterFontSize) {
        squareControl.style.height = squareHeight.toString() + "px";
        squareControl.style.width = squareControl.style.height;
        if (valueControl !== null) {
            valueControl.style.fontSize = letterFontSize.toString() + "px";
        }
        if (heightControl !== null) {
            heightControl.style.top = "-" + valueControl.offsetHeight.toString() + "px";
            heightControl.style.left = String(squareHeight - heightControl.offsetWidth) + "px";
        }
    }

    function setStartupSquare(flag) {
        if (typeof(flag) === "boolean") {
            startup = flag;
        }
    }

    function setAtMaxHeightSquare(flag) {
        if (typeof(flag) === "boolean") {
            atMaxHeight = flag;
        }
    }

    function setSquareStartup() {
        squareControl.className = originalSquareClassName + " startup";
    }

    function setSquareMaxHeight() {
        squareControl.className = originalSquareClassName + " max";
    }

    function setSquareOriginalValue() {
        if (atMaxHeight) {
            setSquareMaxHeight();
        } else {
            squareControl.className = originalSquareClassName + " originalvalue";
        }
    }

    function setSquareOccupied() {
        squareControl.className = originalSquareClassName + " occupied";
    }

    function setSquareUnoccupied() {
        if (startup) {
            setSquareStartup();
        } else {
            squareControl.className = originalSquareClassName;
        }
    }

    function setSquareDisabled() {
        squareControl.className = originalSquareClassName + " disabled";
    }

    function isLocked() {
        return locked;
    }

    function isClear() {
        return clear;
    }

    function isDisabled() {
        return disabled;
    }

    function setControls() {
        if (valueControl !== null) {
            valueControl.innerText = (currentLetterValue !== null
                ? currentLetterValue
                : "");
        }
        if (heightControl !== null) {
            heightControl.innerText = (currentHeightValue !== null
                ? currentHeightValue.toString()
                : "");
        }
    }

    function lockSquare() {
        if (!isRackSquare()) {
            locked = true;
            squareEventManager.setDroppable(false);
        }
    }

    function unlockSquare() {
        if (!isDisabled()) {
            locked = false;
            squareEventManager.setDroppable(true);
        }
    }

    function blankSquare() {
        originalHeightValue = null;
        originalLetterValue = null;
    }

    function clearSquare() {
        clear = true;
        squareEventManager.setDraggable(false);
        currentLetterValue = originalLetterValue;
        currentHeightValue = originalHeightValue;
        setControls();
        if (hasOriginalValue()) {
            setSquareOriginalValue();
        } else {
            setSquareUnoccupied();
        }
        unlockSquare();
    }

    function unclearSquare(letterValue, heightValue) {
        clear = false;
        squareEventManager.setDraggable(true);
        currentLetterValue = letterValue;
        currentHeightValue = heightValue;
        setControls();
        setSquareOccupied();
        lockSquare();
    }

    function disableSquare() {
        disabled = true;
        clearSquare();
        lockSquare();
        setSquareDisabled();
    }

    function setSquareValues(bothValues) {
        var letterValue = ((bothValues !== null && bothValues.letterValue !== null && typeof(bothValues.letterValue) === "string" && bothValues.letterValue.trim().length > 0)
                ? bothValues.letterValue
                : null),
            heightValue = ((bothValues !== null && bothValues.heightValue !== null && typeof(bothValues.heightValue) === "number" && Number.isInteger(bothValues.heightValue) && bothValues.heightValue > 0)
                ? bothValues.heightValue
                : null);

        if (letterValue === null && heightValue === null) {
            clearSquare();
        } else {
            unclearSquare(letterValue, heightValue);
        }
    }

    function setSquareLetterAndHeight(letterValue, heightValue) {
        setSquareValues({
            letterValue: letterValue,
            heightValue: heightValue
        });
    }

    function setOriginalSquareLetterAndHeight(letterValue, heightValue) {
        if (letterValue !== null && typeof(letterValue) === "string") {
            originalLetterValue = letterValue;
        }
        if (heightValue !== null && typeof(heightValue) === "number" && Number.isInteger(heightValue) && heightValue > 0) {
            originalHeightValue = heightValue;
        }
        clearSquare();
    }

    function setSquareLetter(letterValue) {
        setSquareValues({
            letterValue: letterValue,
            heightValue: null
        });
    }

    function getSquareValues() {
        var letterValue = currentLetterValue,
            heightValue = currentHeightValue;
        return {
            letterValue: letterValue,
            heightValue: heightValue
        };
    }

    function getCurrentHeightValue() {
        if (currentHeightValue === null) {
            return 0;
        }
        return currentHeightValue;
    }

    clearSquare();

    return {
        getSquareEventManager: getSquareEventManager,
        isRackSquare: isRackSquare,
        isBoardSquare: isBoardSquare,
        getRowNumber: getRowNumber,
        getColumnNumber: getColumnNumber,
        getLookupKey: getLookupKey,
        resizeSquare: resizeSquare,
        clearSquare: clearSquare,
        blankSquare: blankSquare,
        setSquareLetterAndHeight: setSquareLetterAndHeight,
        setSquareLetter: setSquareLetter,
        setSquareValues: setSquareValues,
        setOriginalSquareLetterAndHeight: setOriginalSquareLetterAndHeight,
        getSquareValues: getSquareValues,
        isClear: isClear,
        isLocked: isLocked,
        isDisabled: isDisabled,
        disableSquare: disableSquare,
        getCurrentHeightValue: getCurrentHeightValue,
        setStartupSquare: setStartupSquare,
        setAtMaxHeightSquare: setAtMaxHeightSquare
    };
};

window.word_game.AllSquaresManager = function (allSquareControls, originalSquareClassName) {
    var allSquares = [],
        rackSquares = [],
        boardSquares = [],
        allSquaresLookup = {};

    function createLookupKey(rowNumber, columnNumber) {
        return "L" + String(100 * rowNumber + columnNumber);
    }

    function findByLookupKey(lookupKey) {
        return allSquaresLookup[lookupKey];
    }

    function findByRowAndColumnNumber(rowNumber, columnNumber) {
        return findByLookupKey(createLookupKey(rowNumber, columnNumber));
    }

    allSquareControls.forEach(function (squareControl) {
        var square = new window.word_game.SquareManager(squareControl, createLookupKey, findByLookupKey, originalSquareClassName);
        allSquares.push(square);
        if (square.isBoardSquare()) {
            boardSquares.push(square);
        } else if (square.isRackSquare()) {
            rackSquares.push(square);
        }
        allSquaresLookup[square.getLookupKey()] = square;
    });

    function resizeAllSquares(playingAreaHeight) {
        var minSquareHeight = 50,
            squareHeight = Math.floor((playingAreaHeight - 120) / 11),
            letterFontSize;

        if (squareHeight < minSquareHeight) {
            squareHeight = minSquareHeight;
        }

        letterFontSize = 0.6 * squareHeight;

        allSquares.forEach(function (squareManager) {
            squareManager.resizeSquare(squareHeight, letterFontSize);
        });
    }

    function clearRack() {
        rackSquares.forEach(function (squareManager) {
            squareManager.clearSquare();
        });
    }

    function blankBoard() {
        boardSquares.forEach(function (squareManager) {
            squareManager.blankSquare();
        });
    }

    function refreshRackSquare(letterManager) {
        var squareManager = findByRowAndColumnNumber(letterManager.getRowNumber(), 0);
        if (squareManager) {
            squareManager.setSquareLetter(letterManager.getLetterValue());
        }
    }

    function refreshBoardSquare(letterManager) {
        var squareManager = findByRowAndColumnNumber(letterManager.getRowNumber(), letterManager.getColumnNumber());
        if (squareManager) {
            if (letterManager.isDisabled()) {
                squareManager.disableSquare();
            } else {
                squareManager.setStartupSquare(letterManager.isStartup());
                squareManager.setAtMaxHeightSquare(letterManager.isAtMaxHeight());
                squareManager.setOriginalSquareLetterAndHeight(letterManager.getLetterValue(), letterManager.getHeightValue());
            }
        }
    }

    function getAllSquares() {
        return allSquares;
    }

    function findClearRackSquare() {
        var clearSquare = null;
        rackSquares.forEach(function (squareManager) {
            if (clearSquare === null && squareManager.isClear()) {
                clearSquare = squareManager;
            }
        });
        return clearSquare;
    }

    function getTilePlacements() {
        var tilePlacements = [];
        boardSquares.forEach(function (squareManager) {
            if (!squareManager.isClear()) {
                tilePlacements.push({
                    letter: squareManager.getSquareValues().letterValue,
                    row: squareManager.getRowNumber(),
                    column: squareManager.getColumnNumber()
                });
            }
        });
        return tilePlacements;
    }

    function getTilePlacementSquareManagers() {
        var tilePlacementSquareManagers = [];
        boardSquares.forEach(function (squareManager) {
            if (!squareManager.isClear()) {
                tilePlacementSquareManagers.push(squareManager);
            }
        });
        return tilePlacementSquareManagers;
    }

    return {
        resizeAllSquares: resizeAllSquares,
        clearRack: clearRack,
        blankBoard: blankBoard,
        refreshRackSquare: refreshRackSquare,
        refreshBoardSquare: refreshBoardSquare,
        getAllSquares: getAllSquares,
        findClearRackSquare: findClearRackSquare,
        getTilePlacements: getTilePlacements,
        getTilePlacementSquareManagers: getTilePlacementSquareManagers
    };
};

window.word_game.ControlManager = function () {
    var doc = window.document,
        overlay = doc.getElementById("overlay"),
        overlayText = doc.getElementById("overlayText"),
        topBar = doc.getElementById("topBar"),
        bottomBar = doc.getElementById("bottomBar"),
        scoresView = doc.getElementById("scoresView"),
        playingAreaView = doc.getElementById("playingAreaView"),
        serverWarning = doc.getElementById("serverWarning"),
        serverWarningMessage = doc.getElementById("serverWarningMessage"),
        createButton = doc.getElementById("createButton"),
        getGameIdButton = doc.getElementById("getGameIdButton"),
        startGameButton = doc.getElementById("startGameButton"),
        gameId = doc.getElementById("gameId"),
        playerId = doc.getElementById("playerId"),
        playerName = doc.getElementById("playerName"),
        playerNameInput = doc.getElementById("playerNameInput"),
        playerNames = doc.getElementById("playerNames"),
        rulesetHolder = doc.getElementById("rulesetHolder"),
        rulesetSelect = doc.getElementById("rulesetSelect"),
        joinButton = doc.getElementById("joinButton"),
        gameIdInput = doc.getElementById("gameIdInput"),
        joinPlayerNameInput = doc.getElementById("joinPlayerNameInput"),
        getPlayerIdButton = doc.getElementById("getPlayerIdButton"),
        createGameDialog = doc.getElementById("createGameDialog"),
        createGameInitialField = doc.getElementById("playerNameInput"),
        creatingGameDialog = doc.getElementById("creatingGameDialog"),
        joinGameDialog = doc.getElementById("joinGameDialog"),
        joinGameInitialField = doc.getElementById("gameIdInput"),
        gameInitiationButtons = doc.getElementById("gameInitiationButtons"),
        awaitingTurn = doc.getElementById("awaitingTurn"),
        turnsToWaitCaption = doc.getElementById("turnsToWaitCaption"),
        gamePlayButtons = doc.getElementById("gamePlayButtons"),
        playButton = doc.getElementById("playButton"),
        swapButton = doc.getElementById("swapButton"),
        passButton = doc.getElementById("passButton"),
        refreshGameButtons = doc.getElementById("refreshGameButtons"),
        refreshButton = doc.getElementById("refreshButton"),
        confirmPassDialog = doc.getElementById("confirmPassDialog"),
        confirmPassButton = doc.getElementById("confirmPassButton"),
        cancelPassButton = doc.getElementById("cancelPassButton"),
        wordsPlayedDialog = doc.getElementById("wordsPlayedDialog"),
        wordsPlayedMessage = doc.getElementById("wordsPlayedMessage"),
        acceptWordsPlayedButton = doc.getElementById("acceptWordsPlayedButton"),
        gameOverDialog = doc.getElementById("gameOverDialog"),
        gameOverMessage = doc.getElementById("gameOverMessage"),
        acceptGameOverButton = doc.getElementById("acceptGameOverButton"),
        tilesLeftPanel = doc.getElementById("tilesLeftPanel"),
        numberOfTilesLeft = doc.getElementById("numberOfTilesLeft"),
        rackControls = doc.getElementById("rackControls"),
        returnToRackButton = doc.getElementById("returnToRackButton"),
        windowEventManager = new window.word_game.WindowEventManager(),
        createButtonEventManager = new window.word_game.ButtonControlEventManager(createButton),
        getGameIdButtonEventManager = new window.word_game.ButtonControlEventManager(getGameIdButton),
        joinButtonEventManager = new window.word_game.ButtonControlEventManager(joinButton),
        startGameButtonEventManager = new window.word_game.ButtonControlEventManager(startGameButton),
        getPlayerIdButtonEventManager = new window.word_game.ButtonControlEventManager(getPlayerIdButton),
        playButtonEventManager = new window.word_game.ButtonControlEventManager(playButton),
        swapButtonEventManager = new window.word_game.ButtonControlEventManager(swapButton),
        passButtonEventManager = new window.word_game.ButtonControlEventManager(passButton),
        confirmPassButtonEventManager = new window.word_game.ButtonControlEventManager(confirmPassButton),
        cancelPassButtonEventManager = new window.word_game.ButtonControlEventManager(cancelPassButton),
        refreshButtonEventManager = new window.word_game.ButtonControlEventManager(refreshButton),
        acceptWordsPlayedButtonEventManager = new window.word_game.ButtonControlEventManager(acceptWordsPlayedButton),
        acceptGameOverButtonEventManager = new window.word_game.ButtonControlEventManager(acceptGameOverButton),
        returnToRackButtonEventManager = new window.word_game.ButtonControlEventManager(returnToRackButton),
        originalSquareClassName = "square",
        allSquaresManager = new window.word_game.AllSquaresManager(Array.from(doc.getElementsByClassName(originalSquareClassName)), originalSquareClassName),
        scoreNames = Array.from(doc.getElementsByClassName("scoreName")),
        scoreValues = Array.from(doc.getElementsByClassName("scoreValue")),
        turnScore = doc.getElementById("turnScore");

    function hideControl(controlToHide) {
        controlToHide.style.display = "none";
    }

    function showControl(controlToShow) {
        controlToShow.style.display = "block";
    }

    function showOverlay(textToShow) {
        showControl(overlay);
        overlayText.innerText = textToShow;
    }

    function hideOverlay() {
        hideControl(overlay);
    }

    function resizePlayingArea() {
        var topHeight = topBar.offsetHeight,
            bottomHeight = bottomBar.offsetHeight,
            scoresHeight = scoresView.offsetHeight,
            docHeight = window.innerHeight,
            playingAreaHeight = (docHeight - topHeight - bottomHeight - scoresHeight - 20);

        playingAreaView.style.height = playingAreaHeight.toString() + "px";
        allSquaresManager.resizeAllSquares(playingAreaHeight);
    }

    function showWarning(message) {
        serverWarningMessage.innerText = message;
        showControl(serverWarning);
    }

    function hideWarning() {
        hideControl(serverWarning);
    }

    function getWindowEventManager() {
        return windowEventManager;
    }

    function getCreateButtonEventManager() {
        return createButtonEventManager;
    }

    function getGetGameIdButtonEventManager() {
        return getGameIdButtonEventManager;
    }

    function getJoinButtonEventManager() {
        return joinButtonEventManager;
    }

    function getStartGameButtonEventManager() {
        return startGameButtonEventManager;
    }

    function getGetPlayerIdButtonEventManager() {
        return getPlayerIdButtonEventManager;
    }

    function getPlayButtonEventManager() {
        return playButtonEventManager;
    }

    function getSwapButtonEventManager() {
        return swapButtonEventManager;
    }

    function getPassButtonEventManager() {
        return passButtonEventManager;
    }

    function getConfirmPassButtonEventManager() {
        return confirmPassButtonEventManager;
    }

    function getCancelPassButtonEventManager() {
        return cancelPassButtonEventManager;
    }

    function getRefreshButtonEventManager() {
        return refreshButtonEventManager;
    }

    function getAcceptWordsPlayedButtonEventManager() {
        return acceptWordsPlayedButtonEventManager;
    }

    function getAcceptGameOverButtonEventManager() {
        return acceptGameOverButtonEventManager;
    }

    function getReturnToRackButtonEventManager() {
        return returnToRackButtonEventManager;
    }

    function setGameId(newId) {
        gameId.innerText = newId;
    }

    function getGameId() {
        return gameId.innerText;
    }

    function setPlayerId(newId) {
        playerId.innerText = newId;
    }

    function getPlayerId() {
        return playerId.innerText;
    }

    function setPlayerName(newName) {
        playerName.innerText = newName;
    }

    function getPlayerName() {
        return playerName.innerText;
    }

    function setPlayerNames(newNames) {
        playerNames.innerText = newNames;
    }

    function getPlayerNames() {
        return playerNames.innerText;
    }

    function hasPlayerNameInput() {
        return (playerNameInput.value.trim() !== "");
    }

    function getPlayerNameInput() {
        return playerNameInput.value;
    }

    function hasJoinPlayerNameInput() {
        return (joinPlayerNameInput.value.trim() !== "");
    }

    function getJoinPlayerNameInput() {
        return joinPlayerNameInput.value;
    }

    function hasGameIdInput() {
        return (gameIdInput.value.trim() !== "");
    }

    function getGameIdInput() {
        return gameIdInput.value;
    }

    function showCreateGameDialog() {
        showControl(createGameDialog);
        createGameInitialField.focus();
    }

    function hideCreateGameDialog() {
        hideControl(createGameDialog);
    }

    function showCreatingGameDialog() {
        showControl(creatingGameDialog);
    }

    function hideCreatingGameDialog() {
        hideControl(creatingGameDialog);
    }

    function showJoinGameDialog() {
        showControl(joinGameDialog);
        joinGameInitialField.focus();
    }

    function hideJoinGameDialog() {
        hideControl(joinGameDialog);
    }

    function showStartGameButton() {
        showControl(startGameButton);
    }

    function hideStartGameButton() {
        hideControl(startGameButton);
    }

    function showStartRulesets() {
        showControl(rulesetHolder);
    }

    function hideStartRulesets() {
        hideControl(rulesetHolder);
    }

    function hideGameInitiationButtons() {
        hideControl(gameInitiationButtons);
    }

    function showGameInitiationButtons() {
        showControl(gameInitiationButtons);
    }

    function showAwaitingTurn() {
        showControl(awaitingTurn);
    }

    function hideAwaitingTurn() {
        hideControl(awaitingTurn);
    }

    function showConfirmPassDialog() {
        showControl(confirmPassDialog);
    }

    function hideConfirmPassDialog() {
        hideControl(confirmPassDialog);
    }

    function showWordsPlayedDialog() {
        showControl(wordsPlayedDialog);
    }

    function hideWordsPlayedDialog() {
        hideControl(wordsPlayedDialog);
    }

    function showGameOverDialog() {
        showControl(gameOverDialog);
    }

    function hideGameOverDialog() {
        hideControl(gameOverDialog);
    }

    function setTurnsToWaitCaption(newCaption) {
        turnsToWaitCaption.innerText = newCaption;
    }

    function setWordsPlayedMessage(newMessage) {
        wordsPlayedMessage.innerText = newMessage;
    }

    function setGameOverMessage(newMessage) {
        gameOverMessage.innerText = newMessage;
    }

    function showGamePlayButtons() {
        showControl(gamePlayButtons);
    }

    function hideGamePlayButtons() {
        hideControl(gamePlayButtons);
    }

    function showRefreshGameButtons() {
        showControl(refreshGameButtons);
    }

    function hideRefreshGameButtons() {
        hideControl(refreshGameButtons);
    }

    function showTilesLeftPanel() {
        showControl(tilesLeftPanel);
    }

    function hideTilesLeftPanel() {
        hideControl(tilesLeftPanel);
    }

    function showRackControls() {
        showControl(rackControls);
    }

    function hideRackControls() {
        hideControl(rackControls);
    }

    function setNumberOfTilesLeft(numTilesLeft) {
        if (typeof(numTilesLeft) === "number" && Number.isInteger(numTilesLeft) && numTilesLeft > -1) {
            numberOfTilesLeft.innerText = numTilesLeft.toString();
        } else {
            numberOfTilesLeft.innerText = "?";
        }
    }

    function setTurnScore(newTurnScore) {
        turnScore.innerText = newTurnScore;
    }

    function refreshRackSquare(letterManager) {
        allSquaresManager.refreshRackSquare(letterManager);
    }

    function refreshBoardSquare(letterManager) {
        allSquaresManager.refreshBoardSquare(letterManager);
    }

    function clearRack() {
        allSquaresManager.clearRack();
    }

    function blankBoard() {
        allSquaresManager.blankBoard();
    }

    function getAllSquaresManager() {
        return allSquaresManager;
    }

    function clearScores() {
        scoreNames.forEach(function (sq) {
            sq.innerText = "";
        });
        scoreValues.forEach(function (sq) {
            sq.innerText = "";
        });
    }

    function setScore(index, name, value, detail) {
        var scorename = doc.getElementById("scorename_" + index.toString()),
            scorevalue = doc.getElementById("scorevalue_" + index.toString()),
            scoredetail = doc.getElementById("scoredetail_" + index.toString());
        scorename.innerText = name;
        scorevalue.innerText = value.toString();
        scoredetail.innerText = detail.toString();
    }

    function setStartRulesets(rulesets) {
        while (rulesetSelect.options.length) {
            rulesetSelect.remove(0);
        }

        rulesets.forEach(function (set) {
            var option = doc.createElement("option");
            option.text = set;
            rulesetSelect.add(option);
        });
    }

    function getSelectedRuleset() {
        return rulesetSelect.value;
    }

    return {
        showOverlay: showOverlay,
        hideOverlay: hideOverlay,
        resizePlayingArea: resizePlayingArea,
        showWarning: showWarning,
        hideWarning: hideWarning,
        getWindowEventManager: getWindowEventManager,
        getCreateButtonEventManager: getCreateButtonEventManager,
        getGetGameIdButtonEventManager: getGetGameIdButtonEventManager,
        getJoinButtonEventManager: getJoinButtonEventManager,
        getStartGameButtonEventManager: getStartGameButtonEventManager,
        getGetPlayerIdButtonEventManager: getGetPlayerIdButtonEventManager,
        getPlayButtonEventManager: getPlayButtonEventManager,
        getSwapButtonEventManager: getSwapButtonEventManager,
        getPassButtonEventManager: getPassButtonEventManager,
        getConfirmPassButtonEventManager: getConfirmPassButtonEventManager,
        getCancelPassButtonEventManager: getCancelPassButtonEventManager,
        getRefreshButtonEventManager: getRefreshButtonEventManager,
        getAcceptWordsPlayedButtonEventManager: getAcceptWordsPlayedButtonEventManager,
        getAcceptGameOverButtonEventManager: getAcceptGameOverButtonEventManager,
        getReturnToRackButtonEventManager: getReturnToRackButtonEventManager,
        setGameId: setGameId,
        getGameId: getGameId,
        setPlayerId: setPlayerId,
        getPlayerId: getPlayerId,
        setPlayerName: setPlayerName,
        getPlayerNames: getPlayerNames,
        setPlayerNames: setPlayerNames,
        getPlayerName: getPlayerName,
        hasPlayerNameInput: hasPlayerNameInput,
        getPlayerNameInput: getPlayerNameInput,
        hasJoinPlayerNameInput: hasJoinPlayerNameInput,
        getJoinPlayerNameInput: getJoinPlayerNameInput,
        hasGameIdInput: hasGameIdInput,
        getGameIdInput: getGameIdInput,
        showCreateGameDialog: showCreateGameDialog,
        hideCreateGameDialog: hideCreateGameDialog,
        showCreatingGameDialog: showCreatingGameDialog,
        hideCreatingGameDialog: hideCreatingGameDialog,
        showJoinGameDialog: showJoinGameDialog,
        hideJoinGameDialog: hideJoinGameDialog,
        showConfirmPassDialog: showConfirmPassDialog,
        hideConfirmPassDialog: hideConfirmPassDialog,
        showWordsPlayedDialog: showWordsPlayedDialog,
        hideWordsPlayedDialog: hideWordsPlayedDialog,
        showGameOverDialog: showGameOverDialog,
        hideGameOverDialog: hideGameOverDialog,
        showStartGameButton: showStartGameButton,
        hideStartGameButton: hideStartGameButton,
        showStartRulesets: showStartRulesets,
        hideStartRulesets: hideStartRulesets,
        hideGameInitiationButtons: hideGameInitiationButtons,
        showGameInitiationButtons: showGameInitiationButtons, 
        showAwaitingTurn: showAwaitingTurn,
        hideAwaitingTurn: hideAwaitingTurn,
        setTurnsToWaitCaption: setTurnsToWaitCaption,
        setWordsPlayedMessage: setWordsPlayedMessage,
        setGameOverMessage: setGameOverMessage,
        showGamePlayButtons: showGamePlayButtons,
        hideGamePlayButtons: hideGamePlayButtons,
        showRefreshGameButtons: showRefreshGameButtons,
        hideRefreshGameButtons: hideRefreshGameButtons,
        showTilesLeftPanel: showTilesLeftPanel,
        hideTilesLeftPanel: hideTilesLeftPanel,
        showRackControls: showRackControls,
        hideRackControls: hideRackControls,
        setNumberOfTilesLeft: setNumberOfTilesLeft,
        clearScores: clearScores,
        setScore: setScore,
        clearRack: clearRack,
        blankBoard: blankBoard,
        refreshRackSquare: refreshRackSquare,
        refreshBoardSquare: refreshBoardSquare,
        getAllSquaresManager: getAllSquaresManager,
        setStartRulesets: setStartRulesets,
        getSelectedRuleset: getSelectedRuleset,
        setTurnScore: setTurnScore
    };
};

window.word_game.ApiEventManager = function () {
    var onStart = null,
        onFinish = null,
        onSuccess = null,
        onFailure = null,
        onValidationError = null;

    function setOnStart(callback) {
        if (typeof(callback) === "function") {
            onStart = callback;
        }
    }

    function raiseOnStart() {
        if (typeof(onStart) === "function") {
            onStart();
        }
    }

    function setOnFinish(callback) {
        if (typeof(callback) === "function") {
            onFinish = callback;
        }
    }

    function raiseOnFinish() {
        if (typeof(onFinish) === "function") {
            onFinish();
        }
    }

    function setOnSuccess(callback) {
        if (typeof(callback) === "function") {
            onSuccess = callback;
        }
    }

    function raiseOnSuccess(result) {
        if (typeof(onSuccess) === "function") {
            onSuccess(result);
        }
    }

    function setOnFailure(callback) {
        if (typeof(callback) === "function") {
            onFailure = callback;
        }
    }

    function raiseOnFailure(error) {
        if (typeof(onFailure) === "function") {
            onFailure(error);
        }
    }

    function setOnValidationError(callback) {
        if (typeof(callback) === "function") {
            onValidationError = callback;
        }
    }

    function raiseOnValidationError(error) {
        if (typeof(onValidationError) === "function") {
            onValidationError(error);
        }
    }

    return {
        setOnStart: setOnStart,
        setOnFinish: setOnFinish,
        setOnSuccess: setOnSuccess,
        setOnFailure: setOnFailure,
        setOnValidationError: setOnValidationError,
        raiseOnStart: raiseOnStart,
        raiseOnFinish: raiseOnFinish,
        raiseOnSuccess: raiseOnSuccess,
        raiseOnFailure: raiseOnFailure,
        raiseOnValidationError: raiseOnValidationError
    };
};

window.word_game.ApiManager = function () {
    var apiUrl = "../game/",
        validationErrorText = "ValidationError",
        createGameEventManager = new window.word_game.ApiEventManager(),
        joinGameEventManager = new window.word_game.ApiEventManager(),
        creatingGameEventManager = new window.word_game.ApiEventManager(),
        startGameEventManager = new window.word_game.ApiEventManager(),
        awaitingTurnEventManager = new window.word_game.ApiEventManager(),
        playTurnEventManager = new window.word_game.ApiEventManager(),
        passTurnEventManager = new window.word_game.ApiEventManager(),
        scoreTurnEventManager = new window.word_game.ApiEventManager(),
        fetcher = window.fetch;

    function handleHttpErrors(response) {
        if (!response.ok) {
            throw new Error("HTTP Error: " + response.statusText);
        }
        return response;
    }

    function safeTrim(t) {
        if (typeof(t) === "string" && t !== null) {
            return t.trim();
        }
        return "";
    }

    function getValidationError(serverData) {
        if (serverData && serverData.errorResult && serverData.errorResult.errorType === validationErrorText) {
            return {
                message: safeTrim(serverData.errorResult.errorMessage)
            };
        }
        return null;
    }

    function getCustomErrorMessage(serverData) {
        if (serverData && serverData.errorResult && serverData.errorResult.errorType !== validationErrorText) {
            return safeTrim(serverData.errorResult.errorMessage);
        }
        return "";
    }

    function handleCustomErrors(serverData) {
        var errMsg = getCustomErrorMessage(serverData);
        if (errMsg.length > 0) {
            throw new Error("Server Error: " + errMsg);
        }
        return serverData;
    }

    function getDataFromServer(relativeUrl, eventManager) {
        eventManager.raiseOnStart();
        fetcher(apiUrl + relativeUrl)
            .then(handleHttpErrors)
            .then(function (response) {
                return response.json();
            })
            .then(handleCustomErrors)
            .then(function (serverData) {
                var validationError = getValidationError(serverData);
                if (validationError === null) {
                    eventManager.raiseOnSuccess(serverData);
                } else {
                    eventManager.raiseOnValidationError(validationError);
                }
            })
            .catch(function (error) {
                eventManager.raiseOnFailure(error);
            })
            .finally(function () {
                eventManager.raiseOnFinish();
            });
    }

    function sendDataToServer(relativeUrl, serverData, eventManager) {
        eventManager.raiseOnStart();
        fetcher(apiUrl + relativeUrl, {
            method: 'POST',
            body: JSON.stringify(serverData),
            headers: {
                'Content-Type': 'application/json'
            }
        })
            .then(handleHttpErrors)
            .then(function (response) {
                return response.json();
            })
            .then(handleCustomErrors)
            .then(function (serverData) {
                var validationError = getValidationError(serverData);
                if (validationError === null) {
                    eventManager.raiseOnSuccess(serverData);
                } else {
                    eventManager.raiseOnValidationError(validationError);
                }
            })
            .catch(function (error) {
                eventManager.raiseOnFailure(error);
            })
            .finally(function () {
                eventManager.raiseOnFinish();
            });
    }

    function getCreateGameEventManager() {
        return createGameEventManager;
    }

    function createGame(playerName) {
        getDataFromServer("create?name=" + encodeURI(playerName), createGameEventManager);
    }

    function getJoinGameEventManager() {
        return joinGameEventManager;
    }

    function joinGame(gameId, playerName) {
        getDataFromServer("join?gameId=" + encodeURI(gameId) + "&name=" + encodeURI(playerName), joinGameEventManager);
    }

    function getCreatingGameEventManager() {
        return creatingGameEventManager;
    }

    function creatingGame(gameId, playerId) {
        getDataFromServer("creating?gameId=" + encodeURI(gameId) + "&playerId=" + encodeURI(playerId), creatingGameEventManager);
    }

    function getStartGameEventManager() {
        return startGameEventManager;
    }

    function startGame(gameId, playerId, ruleset) {
        getDataFromServer("start?gameId=" + encodeURI(gameId) + "&playerId=" + encodeURI(playerId) + "&ruleset=" + ruleset, startGameEventManager);
    }

    function getAwaitingTurnEventManager() {
        return awaitingTurnEventManager;
    }

    function awaitingTurn(gameId, playerId) {
        getDataFromServer("wait?gameId=" + encodeURI(gameId) + "&playerId=" + encodeURI(playerId), awaitingTurnEventManager);
    }

    function getPlayTurnEventManager() {
        return playTurnEventManager;
    }

    function getScoreTurnEventManager() {
        return scoreTurnEventManager;
    }

    function playTurn(gameId, playerId, tilePlacements) {
        var playInput = {
            gameId: gameId,
            playerId: playerId,
            tilePlacements: tilePlacements
        };
        sendDataToServer("play", playInput, playTurnEventManager);
    }

    function scoreTurn(gameId, playerId, tilePlacements) {
        var tryPlayInput = {
            gameId: gameId,
            playerId: playerId,
            tilePlacements: tilePlacements
        };
        sendDataToServer("tryplay", tryPlayInput, scoreTurnEventManager);
    }

    function getPassTurnEventManager() {
        return passTurnEventManager;
    }

    function passTurn(gameId, playerId) {
        getDataFromServer("pass?gameId=" + encodeURI(gameId) + "&playerId=" + encodeURI(playerId), passTurnEventManager);
    }

    return {
        getCreateGameEventManager: getCreateGameEventManager,
        createGame: createGame,
        getJoinGameEventManager: getJoinGameEventManager,
        joinGame: joinGame,
        getCreatingGameEventManager: getCreatingGameEventManager,
        creatingGame: creatingGame,
        getStartGameEventManager: getStartGameEventManager,
        startGame: startGame,
        getAwaitingTurnEventManager: getAwaitingTurnEventManager,
        awaitingTurn: awaitingTurn,
        getPlayTurnEventManager: getPlayTurnEventManager,
        playTurn: playTurn,
        getPassTurnEventManager: getPassTurnEventManager,
        scoreTurn: scoreTurn,
        getScoreTurnEventManager: getScoreTurnEventManager,
        passTurn: passTurn
    };
};

window.word_game.PollingEventManager = function (refreshManually) {
    var polling = false,
        timerId = null,
        pollingInterval = null,
        onPollingIntervalEnd = null;

    function usingManualPolling() {
        return (typeof(refreshManually) === "boolean" && refreshManually);
    }

    function usingAutoPolling() {
        return !usingManualPolling();
    }

    function setPollingInterval(interval) {
        if (typeof(interval) === "number" && Number.isInteger(interval) && interval > 0) {
            pollingInterval = interval;
        }
    }

    function setOnPollingIntervalEnd(callback) {
        if (typeof(callback) === "function") {
            onPollingIntervalEnd = callback;
        }
    }

    function startPolling() {
        polling = true;
        if (usingAutoPolling() && timerId === null && pollingInterval !== null && onPollingIntervalEnd !== null) {
            timerId = window.setInterval(onPollingIntervalEnd, pollingInterval);
        }
    }

    function stopPolling() {
        polling = false;
        if (usingAutoPolling() && timerId !== null) {
            window.clearInterval(timerId);
            timerId = null;
        }
    }

    function invokeManually() {
        if (usingManualPolling() && polling && onPollingIntervalEnd !== null) {
            onPollingIntervalEnd();
        }
    }

    return {
        setPollingInterval: setPollingInterval,
        setOnPollingIntervalEnd: setOnPollingIntervalEnd,
        startPolling: startPolling,
        stopPolling: stopPolling,
        invokeManually: invokeManually,
        usingManualPolling: usingManualPolling
    };
};

window.word_game.PollingManager = function (refreshManually) {
    var creatingGameEventManager = new window.word_game.PollingEventManager(false),
        awaitingTurnEventManager = new window.word_game.PollingEventManager(refreshManually),
        allManualPollers = [];

    if (creatingGameEventManager.usingManualPolling()) {
        allManualPollers.push(creatingGameEventManager);
    }

    if (awaitingTurnEventManager.usingManualPolling()) {
        allManualPollers.push(awaitingTurnEventManager);
    }

    function getCreatingGameEventManager() {
        return creatingGameEventManager;
    }

    function getAwaitingTurnEventManager() {
        return awaitingTurnEventManager;
    }

    function invokeManually() {
        allManualPollers.forEach(function (pollingEventManager) {
            pollingEventManager.invokeManually();
        });
    }

    return {
        getCreatingGameEventManager: getCreatingGameEventManager,
        getAwaitingTurnEventManager: getAwaitingTurnEventManager,
        invokeManually: invokeManually
    };
};

window.word_game.StateManager = function () {
    var gameId = null,
        playerId = null,
        playerName = null,
        lastKnownTurn = -1,
        isMyTurn = false;

    function getGameId() {
        return gameId;
    }

    function setGameId(newValue) {
        gameId = newValue;
    }

    function getPlayerId() {
        return playerId;
    }

    function setPlayerId(newValue) {
        playerId = newValue;
    }

    function getPlayerName() {
        return playerName;
    }

    function setPlayerName(newValue) {
        playerName = newValue;
    }

    function getLastKnownTurn() {
        return lastKnownTurn;
    }

    function setLastKnownTurn(newValue) {
        lastKnownTurn = newValue;
    }

    function getIsMyTurn() {
        return isMyTurn;
    }

    function setIsMyTurn(newValue) {
        if (newValue !== undefined) {
            isMyTurn = newValue;
        }
    }

    return {
        getGameId: getGameId,
        setGameId: setGameId,
        getPlayerId: getPlayerId,
        setPlayerId: setPlayerId,
        getPlayerName: getPlayerName,
        setPlayerName: setPlayerName,
        getLastKnownTurn: getLastKnownTurn,
        setLastKnownTurn: setLastKnownTurn,
        getIsMyTurn: getIsMyTurn,
        setIsMyTurn: setIsMyTurn
    };
};

window.word_game.PlayerManager = function (player) {
    var name = "",
        score = 0,
        turnsToWait = 0,
        lastScore = 0,
        lastWords = "",
        lastAction = "";

    if (typeof(player) === "object") {
        if (typeof(player.playerName) === "string") {
            name = player.playerName;
        }
        if (typeof(player.score) === "number") {
            score = player.score;
        }
        if (typeof(player.turnsToWait) === "number") {
            turnsToWait = player.turnsToWait;
        }
        if (typeof(player.lastScore) === "number") {
            lastScore = player.lastScore;
        }
        if (typeof(player.lastWords) === "object" && player.lastWords !== null && player.lastWords.length > 0) {
            lastWords = player.lastWords.join();
        }
        lastAction = player.lastAction;
    }

    function getName() {
        return name;
    }

    function getScore() {
        return score;
    }

    function getTurnsToWait() {
        return turnsToWait;
    }

    function getLastTurn() {
        if (lastAction === "Play") {
            return "Played " + lastWords + " for " + lastScore + " points";
        }
        if (lastAction === "Swap") {
            return "Swapped a letter";
        }
        if (lastAction === "Pass") {
            return "Passed";
        }
        if (lastAction === "EndGame") {
            return "Subtracted " + lastScore + " for tiles remaining";
        }

        return "";
    }

    return {
        getName: getName,
        getScore: getScore,
        getTurnsToWait: getTurnsToWait,
        getLastTurn: getLastTurn
    };
};

window.word_game.PlayerListManager = function (players) {
    var currentPlayer = null,
        otherPlayers = [],
        playerCount = 0,
        otherPlayerCount = 0;

    if (players && players.length && players.length > 0) {
        players.forEach(function (player) {
            playerCount += 1;
            if (player.isCurrentPlayer) {
                currentPlayer = new window.word_game.PlayerManager(player);
            } else {
                otherPlayerCount += 1;
                otherPlayers.push(new window.word_game.PlayerManager(player));
            }
        });
    }

    function getWinningPlayer() {
        var maxScore = currentPlayer.getScore();
        var winningPlayer = "you";
        otherPlayers.forEach(function (player) {
            if (player.getScore() > maxScore) {
                maxScore = player.getScore();
                winningPlayer = player.getName();
            } else if (player.getScore() === maxScore) {
                winningPlayer += " and " + player.getScore();
            }
        });

        return winningPlayer;
    }

    function getCurrentPlayer() {
        return currentPlayer;
    }

    function getOtherPlayers() {
        return otherPlayers;
    }

    function getPlayerCount() {
        return playerCount;
    }

    function getOtherPlayerCount() {
        return otherPlayerCount;
    }

    function hasPlayers() {
        return (playerCount > 0);
    }

    function hasOtherPlayers() {
        return (otherPlayerCount > 0);
    }

    function hasMultipleOtherPlayers() {
        return (otherPlayerCount > 1);
    }

    function getOtherPlayerNames() {
        var otherPlayerNames = "";

        otherPlayers.forEach(function (player) {
            if (otherPlayerNames.length > 0) {
                otherPlayerNames += " and ";
            }
            otherPlayerNames += player.getName();
        });

        return otherPlayerNames;
    }

    return {
        getCurrentPlayer: getCurrentPlayer,
        getOtherPlayers: getOtherPlayers,
        getPlayerCount: getPlayerCount,
        getOtherPlayerCount: getOtherPlayerCount,
        hasPlayers: hasPlayers,
        hasOtherPlayers: hasOtherPlayers,
        hasMultipleOtherPlayers: hasMultipleOtherPlayers,
        getOtherPlayerNames: getOtherPlayerNames,
        getWinningPlayer: getWinningPlayer
    };
};

window.word_game.CreatingGameManager = function (creatingGameData) {
    var playerListManager = new window.word_game.PlayerListManager(creatingGameData.players);

    function getPlayerListManager() {
        return playerListManager;
    }

    function canStart() {
        return creatingGameData.canStart;
    }

    function hasStarted() {
        return creatingGameData.hasStarted;
    }

    return {
        getPlayerListManager: getPlayerListManager,
        canStart: canStart,
        hasStarted: hasStarted
    };
};

window.word_game.LetterManager = function (row, column, letter, height, squareType) {
    var rowNumber = row,
        columnNumber = (column !== null
            ? column
            : 0),
        letterValue = ((typeof(letter.trim) === "function" && letter.trim().length > 0)
            ? letter
            : null),
        heightValue = ((typeof(height) === "number" && Number.isInteger(height) && height > 0)
            ? height
            : null),
        disabled = (squareType === "Unplayable"),
        startup = (squareType === "Starting"),
        atMaxHeight = (squareType === "MaxHeight");

    function getRowNumber() {
        return rowNumber;
    }

    function getColumnNumber() {
        return columnNumber;
    }

    function getLetterValue() {
        return letterValue;
    }

    function getHeightValue() {
        return heightValue;
    }

    function isDisabled() {
        return disabled;
    }

    function isStartup() {
        return startup;
    }

    function isAtMaxHeight() {
        return atMaxHeight;
    }

    return {
        getRowNumber: getRowNumber,
        getColumnNumber: getColumnNumber,
        getLetterValue: getLetterValue,
        getHeightValue: getHeightValue,
        isDisabled: isDisabled,
        isStartup: isStartup,
        isAtMaxHeight: isAtMaxHeight
    };
};

window.word_game.RackManager = function (rack) {
    var letters = [], rowNumber = 0;

    if (rack && rack.length) {
        rack.forEach(function (letter) {
            rowNumber += 1;
            letters.push(new window.word_game.LetterManager(rowNumber, null, letter, null, null));
        });
    }

    function getLetters() {
        return letters;
    }

    return {
        getLetters: getLetters
    };
};

window.word_game.BoardManager = function (board) {
    var letters = [], rowNumber = 0, columnNumber = 0;

    if (board && board.length) {
        board.forEach(function (row) {
            rowNumber += 1;
            columnNumber = 0;
            row.forEach(function (boardSquare) {
                columnNumber += 1;
                letters.push(new window.word_game.LetterManager(rowNumber, columnNumber, boardSquare.letter, boardSquare.height, boardSquare.squareType));
            });
        });
    }

    function getLetters() {
        return letters;
    }

    return {
        getLetters: getLetters
    };
};

window.word_game.TurnManager = function (stateManager, turnData) {
    var playerListManager = new window.word_game.PlayerListManager(turnData.players),
        rackManager = new window.word_game.RackManager(turnData.rack),
        boardManager = new window.word_game.BoardManager(turnData.board),
        newTurn = (turnData.turnNumber !== stateManager.getLastKnownTurn()),
        lastWords = turnData.lastWords,
        lastScore = turnData.lastScore,
        gameOver = turnData.gameOver;

    stateManager.setLastKnownTurn(turnData.turnNumber);
    stateManager.setIsMyTurn(turnData.isMyTurn);

    function getPlayerListManager() {
        return playerListManager;
    }

    function getRackManager() {
        return rackManager;
    }

    function getBoardManager() {
        return boardManager;
    }

    function isNew() {
        return newTurn;
    }

    function isGameOver() {
        return gameOver;
    }

    function isMyTurn() {
        return turnData.isMyTurn;
    }

    function getTilesLeft() {
        return turnData.tilesLeft;
    }

    function getLastWords() {
        return lastWords;
    }

    function getLastScore() {
        return lastScore;
    }

    return {
        getPlayerListManager: getPlayerListManager,
        getRackManager: getRackManager,
        getBoardManager: getBoardManager,
        isNew: isNew,
        isGameOver: isGameOver,
        isMyTurn: isMyTurn,
        getTilesLeft: getTilesLeft,
        getLastWords: getLastWords,
        getLastScore: getLastScore
    };
};

window.word_game.GameManager = (function () {
    var controlManager,
        apiManager,
        pollingManager,
        stateManager;

    function window_OnLoad() {
        controlManager.resizePlayingArea();
    }

    function window_OnResize() {
        controlManager.resizePlayingArea();
    }

    function apiCall_OnStart() {
        controlManager.showOverlay("Working, please wait...");
    }

    function apiCall_OnFinish() {
        controlManager.hideOverlay();
    }

    function apiCall_CreateGame_OnSuccess(result) {
        stateManager.setGameId(result.gameId);
        stateManager.setPlayerId(result.playerId);
        stateManager.setPlayerName(controlManager.getPlayerNameInput());
        controlManager.setGameId(stateManager.getGameId());
        controlManager.setPlayerId(stateManager.getPlayerId());
        controlManager.setPlayerName(stateManager.getPlayerName());
        controlManager.hideCreateGameDialog();
        controlManager.hideGameInitiationButtons();
        controlManager.hideStartGameButton();
        controlManager.hideStartRulesets();
        controlManager.showCreatingGameDialog();
        controlManager.setStartRulesets(result.rulesets);
        pollingManager.getCreatingGameEventManager().startPolling();
    }

    function apiCall_CreateGame_OnFailure(error) {
        controlManager.hideCreateGameDialog();
        controlManager.showWarning(error.message);
    }

    function apiCall_JoinGame_OnSuccess(result) {
        stateManager.setPlayerId(result.playerId);
        stateManager.setGameId(controlManager.getGameIdInput());
        stateManager.setPlayerName(controlManager.getJoinPlayerNameInput());
        controlManager.setGameId(stateManager.getGameId());
        controlManager.setPlayerId(stateManager.getPlayerId());
        controlManager.setPlayerName(stateManager.getPlayerName());
        controlManager.hideJoinGameDialog();
        controlManager.hideGameInitiationButtons();
        controlManager.hideStartGameButton();
        controlManager.hideStartRulesets();
        controlManager.showCreatingGameDialog();
        pollingManager.getCreatingGameEventManager().startPolling();
    }

    function apiCall_JoinGame_OnFailure(error) {
        controlManager.hideJoinGameDialog();
        controlManager.showWarning(error.message);
    }

    function updateScores(playerListManager) {
        var iPlayer = 1;
        if (playerListManager.hasPlayers()) {
            controlManager.clearScores();
            var currentPlayer = playerListManager.getCurrentPlayer();
            controlManager.setScore(iPlayer, currentPlayer.getName(), currentPlayer.getScore(), currentPlayer.getLastTurn());
            playerListManager.getOtherPlayers().forEach(function (player) {
                iPlayer += 1;
                controlManager.setScore(iPlayer, player.getName(), player.getScore(), player.getLastTurn());
            });
        }
    }

    function updateTilesLeft(turnManager) {
        controlManager.showTilesLeftPanel();
        controlManager.setNumberOfTilesLeft(turnManager.getTilesLeft());
    }

    function updateJoiningPlayerNames(playerListManager) {
        var playerNames;
        if (playerListManager.hasOtherPlayers()) {
            playerNames = playerListManager.getOtherPlayerNames();
            if (playerListManager.hasMultipleOtherPlayers()) {
                playerNames += " have ";
            } else {
                playerNames += " has ";
            }
            playerNames += "joined the game";
            controlManager.setPlayerNames(playerNames);
        }
    }

    function apiCall_CreatingGame_OnSuccess(result) {
        var creatingGameManager = new window.word_game.CreatingGameManager(result);
        updateScores(creatingGameManager.getPlayerListManager());
        updateJoiningPlayerNames(creatingGameManager.getPlayerListManager());
        if (creatingGameManager.canStart()) {
            controlManager.showStartGameButton();
            controlManager.showStartRulesets();
        }
        if (creatingGameManager.hasStarted()) {
            pollingManager.getCreatingGameEventManager().stopPolling();
            localStorage.setItem('gameId', stateManager.getGameId());
            localStorage.setItem('playerId', stateManager.getPlayerId());
            controlManager.blankBoard();
            controlManager.hideCreatingGameDialog();
            pollingManager.getAwaitingTurnEventManager().startPolling();
        }
    }

    function apiCall_CreatingGame_OnFailure(error) {
        pollingManager.getCreatingGameEventManager().stopPolling();
        controlManager.hideCreatingGameDialog();
        controlManager.showWarning(error.message);
    }

    function apiCall_StartGame_OnSuccess() {
        pollingManager.getCreatingGameEventManager().stopPolling();

        localStorage.setItem('gameId', stateManager.getGameId());
        localStorage.setItem('playerId', stateManager.getPlayerId());

        controlManager.blankBoard();
        controlManager.hideCreatingGameDialog();
        pollingManager.getAwaitingTurnEventManager().startPolling();
    }

    function apiCall_StartGame_OnFailure(error) {
        controlManager.hideCreatingGameDialog();
        controlManager.showWarning(error.message);
    }

    function makeTurnsToWaitCaption(turnsToWait) {
        var result = "Waiting for " + turnsToWait.toString();
        if (turnsToWait > 1) {
            result += " players to take their turns";
        } else {
            result += " player to take their turn";
        }
        return result;
    }

    function makeGameOverCaption(playerListManager) {
        return "The game is over. The winner is " + playerListManager.getWinningPlayer() + ".";
    }

    function refreshRackAndBoard(turnManager) {
        var hasLettersOnRack = false;
        controlManager.clearRack();
        turnManager.getRackManager().getLetters().forEach(function (letter) {
            hasLettersOnRack = true;
            controlManager.refreshRackSquare(letter);
        });
        turnManager.getBoardManager().getLetters().forEach(function (letter) {
            controlManager.refreshBoardSquare(letter);
        });
        if (hasLettersOnRack && turnManager.isMyTurn()) {
            controlManager.showRackControls();
        } else {
            controlManager.hideRackControls();
        }
    }

    function apiCall_AwaitingTurn_OnSuccess(result) {
        var turnManager = new window.word_game.TurnManager(stateManager, result);

        updateScores(turnManager.getPlayerListManager());
        updateTilesLeft(turnManager);
        if (turnManager.isGameOver()) {
            localStorage.clear();
            pollingManager.getAwaitingTurnEventManager().stopPolling();
            controlManager.hideAwaitingTurn();
            controlManager.hideGamePlayButtons();
            refreshRackAndBoard(turnManager);
            controlManager.setGameOverMessage(makeGameOverCaption(turnManager.getPlayerListManager()));
            controlManager.showGameOverDialog();
            controlManager.showGameInitiationButtons();
        } else {
            if (turnManager.isNew()) {
                //Refresh the screen
                controlManager.hideGamePlayButtons();
                controlManager.setTurnsToWaitCaption(makeTurnsToWaitCaption(turnManager.getPlayerListManager().getCurrentPlayer().getTurnsToWait()));
                controlManager.showAwaitingTurn();
                refreshRackAndBoard(turnManager);
            }
            if (turnManager.isMyTurn()) {
                //Stop polling for new turns, and play
                pollingManager.getAwaitingTurnEventManager().stopPolling();
                controlManager.hideAwaitingTurn();
                controlManager.showGamePlayButtons();
            }
        }
    }

    function apiCall_AwaitingTurn_OnFailure(error) {
        pollingManager.getAwaitingTurnEventManager().stopPolling();
        controlManager.showWarning(error.message);
    }

    function formatWordsPlayedMessage(turnManager) {
        var words = turnManager.getLastWords(),
            score = turnManager.getLastScore();

        return "You played " + words.join() + " for " + score.toString() + " points";
    }

    function apiCall_PlayTurn_OnSuccess(result) {
        var turnManager = new window.word_game.TurnManager(stateManager, result);

        //Refresh the screen
        updateScores(turnManager.getPlayerListManager());
        updateTilesLeft(turnManager);
        controlManager.hideGamePlayButtons();
        controlManager.setTurnScore("");

        if (turnManager.isGameOver()) {
            localStorage.clear();
            refreshRackAndBoard(turnManager);

            controlManager.setWordsPlayedMessage(formatWordsPlayedMessage(turnManager));
            controlManager.showWordsPlayedDialog();

            controlManager.setGameOverMessage(makeGameOverCaption(turnManager.getPlayerListManager()));
            controlManager.showGameOverDialog();
            controlManager.showGameInitiationButtons();
        } else {
            controlManager.setTurnsToWaitCaption(makeTurnsToWaitCaption(turnManager.getPlayerListManager().getCurrentPlayer().getTurnsToWait()));
            controlManager.showAwaitingTurn();
            refreshRackAndBoard(turnManager);

            controlManager.setWordsPlayedMessage(formatWordsPlayedMessage(turnManager));
            controlManager.showWordsPlayedDialog();

            //Start polling for new turns
            pollingManager.getAwaitingTurnEventManager().startPolling();
        }
    }

    function apiCall_PlayTurn_OnFailure(error) {
        controlManager.showWarning(error.message);
    }

    function apiCall_ScoreTurn_OnSuccess(result) {
        var turnManager = new window.word_game.TurnManager(stateManager, result);
        controlManager.setTurnScore(turnManager.getLastScore());
    }

    function apiCall_ScoreTurn_OnFailure() {
        controlManager.setTurnScore("");
    }

    function apiCall_PassTurn_OnSuccess(result) {
        var turnManager = new window.word_game.TurnManager(stateManager, result);

        //Refresh the screen
        updateScores(turnManager.getPlayerListManager());
        updateTilesLeft(turnManager);
        controlManager.hideGamePlayButtons();
        controlManager.setTurnScore("");
        controlManager.setTurnsToWaitCaption(makeTurnsToWaitCaption(turnManager.getPlayerListManager().getCurrentPlayer().getTurnsToWait()));
        controlManager.showAwaitingTurn();
        refreshRackAndBoard(turnManager);

        //Start polling for new turns
        pollingManager.getAwaitingTurnEventManager().startPolling();
    }

    function apiCall_PassTurn_OnFailure(error) {
        controlManager.showWarning(error.message);
    }

    function createButton_OnClick() {
        controlManager.showCreateGameDialog();
    }

    function joinButton_OnClick() {
        controlManager.showJoinGameDialog();
    }

    //For a player without a game id, who is trying to create a game
    function getGameIdButton_OnClick() {
        if (!controlManager.hasPlayerNameInput()) {
            controlManager.hideCreateGameDialog();
            controlManager.showWarning("You must enter a player name");
        } else {
            apiManager.createGame(controlManager.getPlayerNameInput());
        }
    }

    //For a player with a game id, who is trying to join the game
    function getPlayerIdButton_OnClick() {
        if (!controlManager.hasGameIdInput()) {
            controlManager.hideJoinGameDialog();
            controlManager.showWarning("You must enter a game id");
        } else if (!controlManager.hasJoinPlayerNameInput()) {
            controlManager.hideJoinGameDialog();
            controlManager.showWarning("You must enter a player name");
        } else {
            apiManager.joinGame(controlManager.getGameIdInput(), controlManager.getJoinPlayerNameInput());
        }
    }

    function creatingGame_OnPollingIntervalEnd() {
        apiManager.creatingGame(stateManager.getGameId(), stateManager.getPlayerId());
    }

    function awaitingTurn_OnPollingIntervalEnd() {
        apiManager.awaitingTurn(stateManager.getGameId(), stateManager.getPlayerId());
    }

    function startGameButton_OnClick() {
        apiManager.startGame(stateManager.getGameId(), stateManager.getPlayerId(), controlManager.getSelectedRuleset());
    }

    function moveSquareFromRackToRack(targetSquareManager, sourceSquareManager) {
        //Just swap them, for now
        var originalTargetValues = targetSquareManager.getSquareValues(),
            originalSourceValues = sourceSquareManager.getSquareValues();
        targetSquareManager.setSquareValues(originalSourceValues);
        sourceSquareManager.setSquareValues(originalTargetValues);
    }

    function moveSquareFromRackToBoard(targetSquareManager, sourceSquareManager) {
        var originalSourceValues = sourceSquareManager.getSquareValues(),
            originalTargetHeightValue = targetSquareManager.getCurrentHeightValue();
        if (!targetSquareManager.isLocked()) {
            sourceSquareManager.clearSquare();
            targetSquareManager.setSquareLetterAndHeight(originalSourceValues.letterValue, originalTargetHeightValue + 1);
        }
    }

    function checkClearRackSquare(rackSquareManager) {
        if (!rackSquareManager.isClear()) {
            return controlManager.getAllSquaresManager().findClearRackSquare();
        }
        return rackSquareManager;
    }

    function moveSquareFromBoardToRack(targetSquareManager, sourceSquareManager) {
        var originalSourceValues = sourceSquareManager.getSquareValues();
        if (targetSquareManager !== null) {
            sourceSquareManager.clearSquare();
            targetSquareManager.setSquareLetter(originalSourceValues.letterValue);
        }
    }

    function moveSquareFromBoardToBoard(targetSquareManager, sourceSquareManager) {
        var originalSourceValues = sourceSquareManager.getSquareValues(),
            originalTargetHeightValue = targetSquareManager.getCurrentHeightValue();
        if (!targetSquareManager.isLocked()) {
            sourceSquareManager.clearSquare();
            targetSquareManager.setSquareLetterAndHeight(originalSourceValues.letterValue, originalTargetHeightValue + 1);
        }
    }

    function square_OnDrop(targetSquareManager, sourceSquareManager) {
        var updateScore = true;

        if (targetSquareManager.isRackSquare() && sourceSquareManager.isRackSquare()) {
            moveSquareFromRackToRack(targetSquareManager, sourceSquareManager);
            updateScore = false;
        } else if (targetSquareManager.isBoardSquare() && sourceSquareManager.isRackSquare()) {
            moveSquareFromRackToBoard(targetSquareManager, sourceSquareManager);
        } else if (targetSquareManager.isRackSquare() && sourceSquareManager.isBoardSquare()) {
            moveSquareFromBoardToRack(checkClearRackSquare(targetSquareManager), sourceSquareManager);
        } else if (targetSquareManager.isBoardSquare() && sourceSquareManager.isBoardSquare()) {
            moveSquareFromBoardToBoard(targetSquareManager, sourceSquareManager);
        }

        if (updateScore && stateManager.getIsMyTurn()) {
            apiManager.scoreTurn(stateManager.getGameId(), stateManager.getPlayerId(), controlManager.getAllSquaresManager().getTilePlacements());
        }
    }

    function playButton_OnClick() {
        apiManager.playTurn(stateManager.getGameId(), stateManager.getPlayerId(), controlManager.getAllSquaresManager().getTilePlacements());
    }

    function refreshButton_OnClick() {
        pollingManager.invokeManually();
    }

    function passButton_OnClick() {
        controlManager.showConfirmPassDialog();
    }

    function confirmPassButton_OnClick() {
        controlManager.hideConfirmPassDialog();
        apiManager.passTurn(stateManager.getGameId(), stateManager.getPlayerId());
    }

    function cancelPassButton_OnClick() {
        controlManager.hideConfirmPassDialog();
    }

    function acceptWordsPlayedButton_OnClick() {
        controlManager.hideWordsPlayedDialog();
    }

    function acceptGameOverButton_OnClick() {
        controlManager.hideGameOverDialog();
    }

    function returnToRackButton_OnClick() {
        controlManager.getAllSquaresManager().getTilePlacementSquareManagers().forEach(function (sourceSquareManager) {
            var targetSquareManager = controlManager.getAllSquaresManager().findClearRackSquare();
            moveSquareFromBoardToRack(targetSquareManager, sourceSquareManager);
        });
        controlManager.setTurnScore("");
    }

    function hideWarning() {
        controlManager.hideWarning();
    }

    function hideCreateGameDialog() {
        controlManager.hideCreateGameDialog();
    }

    function hideCreatingGameDialog() {
        controlManager.hideCreatingGameDialog();
    }

    function hideJoinGameDialog() {
        controlManager.hideJoinGameDialog();
    }

    function init(options) {
        var refreshManually = (typeof(options.refreshManually) === "boolean" && options.refreshManually),
            pollingInterval = 2000;

        controlManager = new window.word_game.ControlManager();
        apiManager = new window.word_game.ApiManager();
        pollingManager = new window.word_game.PollingManager(refreshManually);
        stateManager = new window.word_game.StateManager();

        if (refreshManually) {
            controlManager.showRefreshGameButtons();
        } else {
            controlManager.hideRefreshGameButtons();
        }

        //Wire up event handlers
        controlManager.getWindowEventManager().setOnLoad(window_OnLoad);
        controlManager.getWindowEventManager().setOnResize(window_OnResize);

        controlManager.getCreateButtonEventManager().setOnClick(createButton_OnClick);
        controlManager.getJoinButtonEventManager().setOnClick(joinButton_OnClick);
        controlManager.getGetGameIdButtonEventManager().setOnClick(getGameIdButton_OnClick);
        controlManager.getGetPlayerIdButtonEventManager().setOnClick(getPlayerIdButton_OnClick);
        controlManager.getStartGameButtonEventManager().setOnClick(startGameButton_OnClick);
        controlManager.getPlayButtonEventManager().setOnClick(playButton_OnClick);
        controlManager.getRefreshButtonEventManager().setOnClick(refreshButton_OnClick);
        controlManager.getPassButtonEventManager().setOnClick(passButton_OnClick);
        controlManager.getConfirmPassButtonEventManager().setOnClick(confirmPassButton_OnClick);
        controlManager.getCancelPassButtonEventManager().setOnClick(cancelPassButton_OnClick);
        controlManager.getAcceptWordsPlayedButtonEventManager().setOnClick(acceptWordsPlayedButton_OnClick);
        controlManager.getAcceptGameOverButtonEventManager().setOnClick(acceptGameOverButton_OnClick);
        controlManager.getReturnToRackButtonEventManager().setOnClick(returnToRackButton_OnClick);

        apiManager.getCreateGameEventManager().setOnStart(apiCall_OnStart);
        apiManager.getCreateGameEventManager().setOnFinish(apiCall_OnFinish);
        apiManager.getCreateGameEventManager().setOnSuccess(apiCall_CreateGame_OnSuccess);
        apiManager.getCreateGameEventManager().setOnFailure(apiCall_CreateGame_OnFailure);
        apiManager.getCreateGameEventManager().setOnValidationError(apiCall_CreateGame_OnFailure);

        apiManager.getJoinGameEventManager().setOnStart(apiCall_OnStart);
        apiManager.getJoinGameEventManager().setOnFinish(apiCall_OnFinish);
        apiManager.getJoinGameEventManager().setOnSuccess(apiCall_JoinGame_OnSuccess);
        apiManager.getJoinGameEventManager().setOnFailure(apiCall_JoinGame_OnFailure);
        apiManager.getJoinGameEventManager().setOnValidationError(apiCall_JoinGame_OnFailure);

        pollingManager.getCreatingGameEventManager().setPollingInterval(pollingInterval);
        pollingManager.getCreatingGameEventManager().setOnPollingIntervalEnd(creatingGame_OnPollingIntervalEnd);

        apiManager.getCreatingGameEventManager().setOnSuccess(apiCall_CreatingGame_OnSuccess);
        apiManager.getCreatingGameEventManager().setOnFailure(apiCall_CreatingGame_OnFailure);
        apiManager.getCreatingGameEventManager().setOnValidationError(apiCall_CreatingGame_OnFailure);

        apiManager.getStartGameEventManager().setOnStart(apiCall_OnStart);
        apiManager.getStartGameEventManager().setOnFinish(apiCall_OnFinish);
        apiManager.getStartGameEventManager().setOnSuccess(apiCall_StartGame_OnSuccess);
        apiManager.getStartGameEventManager().setOnFailure(apiCall_StartGame_OnFailure);
        apiManager.getStartGameEventManager().setOnValidationError(apiCall_StartGame_OnFailure);

        pollingManager.getAwaitingTurnEventManager().setPollingInterval(pollingInterval);
        pollingManager.getAwaitingTurnEventManager().setOnPollingIntervalEnd(awaitingTurn_OnPollingIntervalEnd);

        apiManager.getAwaitingTurnEventManager().setOnSuccess(apiCall_AwaitingTurn_OnSuccess);
        apiManager.getAwaitingTurnEventManager().setOnFailure(apiCall_AwaitingTurn_OnFailure);
        apiManager.getAwaitingTurnEventManager().setOnValidationError(apiCall_AwaitingTurn_OnFailure);

        apiManager.getPlayTurnEventManager().setOnStart(apiCall_OnStart);
        apiManager.getPlayTurnEventManager().setOnFinish(apiCall_OnFinish);
        apiManager.getPlayTurnEventManager().setOnSuccess(apiCall_PlayTurn_OnSuccess);
        apiManager.getPlayTurnEventManager().setOnFailure(apiCall_PlayTurn_OnFailure);
        apiManager.getPlayTurnEventManager().setOnValidationError(apiCall_PlayTurn_OnFailure);

        apiManager.getScoreTurnEventManager().setOnSuccess(apiCall_ScoreTurn_OnSuccess);
        apiManager.getScoreTurnEventManager().setOnFailure(apiCall_ScoreTurn_OnFailure);
        apiManager.getScoreTurnEventManager().setOnValidationError(apiCall_ScoreTurn_OnFailure);

        apiManager.getPassTurnEventManager().setOnStart(apiCall_OnStart);
        apiManager.getPassTurnEventManager().setOnFinish(apiCall_OnFinish);
        apiManager.getPassTurnEventManager().setOnSuccess(apiCall_PassTurn_OnSuccess);
        apiManager.getPassTurnEventManager().setOnFailure(apiCall_PassTurn_OnFailure);
        apiManager.getPassTurnEventManager().setOnValidationError(apiCall_PassTurn_OnFailure);

        controlManager.getAllSquaresManager().getAllSquares().forEach(function (squareManager) {
            squareManager.getSquareEventManager().setOnDrop(square_OnDrop);
        });

        if (localStorage.getItem('gameId')) {
            var storedGameId = localStorage.getItem('gameId');
            var storedPlayerId = localStorage.getItem('playerId');
            stateManager.setGameId(storedGameId);
            stateManager.setPlayerId(storedPlayerId);
            controlManager.hideGameInitiationButtons();
            pollingManager.getAwaitingTurnEventManager().startPolling();
        }
    }

    return {
        init: init,
        hideWarning: hideWarning,
        hideCreateGameDialog: hideCreateGameDialog,
        hideCreatingGameDialog: hideCreatingGameDialog,
        hideJoinGameDialog: hideJoinGameDialog
    };

}());