;////Code for sending packets.

#include <GUIButton.au3>
#include <GUIToolbar.au3>
#include <GUIConstantsEx.au3>
#include <ProgressConstants.au3>
#include <StaticConstants.au3>
#include <WindowsConstants.au3>
#include <EditConstants.au3>
#include <NomadMemory.au3>
#include <Array.au3>

Global $kernel32 = DllOpen('kernel32.dll')
Global $pid = ProcessExists('elementclient.exe')
global $realBaseAddress = 0x0098657C
global $sendPacketFunction = 0x005BD7B0

;//Deselect target (example usage of sendPacket)
sendDeselectPacket($pid)

DllClose($kernel32)

Func logOut($toAccount, $pid)
    ;//Sends a packet to log the character from the server
    ;//If toAccount=1, it logs to character select 
    ;//If toAccount=0, it exits completely
    local $packet, $packetSize
    
    $packet = '0100'
    $packet &= _hex($toAccount)
    $packetSize = 6

    sendPacket($packet, $packetSize, $pid)
EndFunc    

Func selectTarget($targetId, $pid)
    ;//Select the NPC/Mob/Player denoted by targetId
    local $packet, $packetSize
    
    $packet = '0200'
    $packet &= _hex($targetId)
    $packetSize = 6
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func regularAttack($afterSkill, $pid)
    ;//Start with regular attacks. $afterskill is 1 if you
    ;//start attacking after using a skill.
    local $packet, $packetSize
    
    $packet = '0300'
    $packet &= _hex($afterSkill, 2)
    $packetSize = 3
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func rezToTown($pid)
    ;//Respawn in town after death
    local $packet, $packetSize
    
    $packet = '0400'
    $packetSize = 2
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func rezWithScroll($pid)
    ;//Respawn in the place you died, costs a rez scroll
    local $packet, $packetSize
    
    $packet = '0500'
    $packetSize = 2
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func pickUpItem($uniqueItemId, $itemTypeId, $pid)
    ;//Picks up an item. uniqueItemId is the unique id belonging
    ;//to the individual item on the ground. itemTypeId is the id for 
    ;//the type of item it is. This would be the same as the last
    ;//part in the url on pwdatabase. example:
    ;//http://www.pwdatabase.com/pwi/items/3044
    ;//the itemTypeId for gold is 3044.
    
    local $packet, $packetSize
    
    $packet = '0600'
    $packet &= _hex($uniqueItemId)
    $packet &= _hex($itemTypeId)
    $packetSize = 10
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func deselectTarget($pid)
    ;//Deselects the currently selected target
    local $packet, $packetSize
    
    $packet = '0800'
    $packetSize = 2
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func updateInvPosition($invPosition, $pid)
    ;//This packet is sent whenever you pick up HH/TT items
    ;//Unsure as to why. Also happens when you find a 
    ;//quest item or equipment.
    local $packet, $packetSize
    
    $packet = '0900'
    $packet &= _hex($invPosition, 2)
    $packetSize = 3
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func swapItemInInv($invIndex1, $invIndex2, $pid)
    ;//Swaps the items in the two given inventory locations
    ;//The index for a standard unexpanded inventory runs from 
    ;//0, top left, to 31, bottom right
    local $packet, $packetSize
    
    $packet = '0C00'
    $packet &= _hex($invIndex1, 2)
    $packet &= _hex($invIndex2, 2)
    $packetSize = 4
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func splitStackItemInInv($invIndexSource, $invIndexDestination, $amount, $pid)
    ;//Splits a stack in your inventory located at invIndexSource
    ;//Take off $amouunt from the stack and place them at invIndexDestination
    ;//The index for a standard unexpanded inventory runs from 
    ;//0, top left, to 31, bottom right    
    local $packet, $packetSize
    
    $packet = '0D00'
    $packet &= _hex($invIndexSource, 2)
    $packet &= _hex($invIndexDestination, 2)
    $packet &= _hex($amount, 4)
    $packetSize = 6
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func dropItemOnFloor($invIndexSource, $amount, $pid)
    ;//Drops the stack located at invIndexSource in your inventory
    ;//onto the floor.
    ;//The index for a standard unexpanded inventory runs from 
    ;//0, top left, to 31, bottom right    
    local $packet, $packetSize
    
    $packet = '0E00'
    $packet &= _hex($invIndexSource, 2)
    $packet &= _hex($amount, 4)
    $packetSize = 5
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func swapEquip($equipIndex1, $equipIndex2, $pid)
    ;//Swaps the items in the two given equipment locations
    ;//The index for equipment runs from 
    ;//0, weapon, to 24, speaker?. This also includes fashion
    ;//Obviously there aren't a lot of equipment types you can swap
    ;//besides rings.
    local $packet, $packetSize
    
    $packet = '1000'
    $packet &= _hex($equipIndex1, 2)
    $packet &= _hex($equipIndex2, 2)
    $packetSize = 4
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func swapEquipWithInv($invIndex, $equipIndex, $pid)
    ;//Swaps the items in the invIndex location with the 
    ;//item in the equipment location
    ;//The index for equipment runs from 
    ;//0, weapon, to 24, speaker?. This also includes fashion
    ;//The index for a standard unexpanded inventory runs from 
    ;//0, top left, to 31, bottom right
    local $packet, $packetSize
    
    $packet = '1100'
    $packet &= _hex($invIndex, 2)
    $packet &= _hex($equipIndex, 2)
    $packetSize = 4
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func dropGold($amount, $pid)
    ;//Drops $amount of gold to floor
    local $packet, $packetSize
    
    $packet = '1400'
    $packet &= _hex($amount)
    $packetSize = 6
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func updateStats($pid)
    ;//Is sent whenever a new item is equipped or stat 
    ;//screen is opened or you level up.
    local $packet, $packetSize
    
    $packet = '1500'
    $packetSize = 2
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func increaseStatsBy($con, $int, $str, $agi, $pid)
    ;//Use this after level up to increase your stats. 
    
    local $packet, $packetSize
    
    $packet = '1600'
    $packet &= _hex($con)
    $packet &= _hex($int)
    $packet &= _hex($str)
    $packet &= _hex($agi)    
    
    $packetSize = 18
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func inviteParty($playerId, $pid)
    ;//Invite playerId to your party.
    local $packet, $packetSize
    
    $packet = '1B00'
    $packet &= _hex($playerId)
    $packetSize = 6
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func acceptPartyInvite($playerId, $partyInviteCounter, $pid)
    ;//Accept an invite from playerId. partyInviteCounter is a counter that
    ;//is kept based on the amount of party invites you've had. See post
    ;//on how to find that value.
    local $packet, $packetSize
    
    $packet = '1C00'
    $packet &= _hex($playerId)
    $packet &= _hex($partyInviteCounter)

    $packetSize = 10
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func refusePartyInvite($playerId, $pid)
    ;//Refuses a party invite from playerId
    local $packet, $packetSize
    
    $packet = '1D00'
    $packet &= _hex($playerId)
    $packetSize = 6
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func leaveParty($pid)
    ;//Leave your current party
    local $packet, $packetSize
    
    $packet = '1E00'
    $packetSize = 2
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func evictFromParty($playerId, $pid)
    ;//Evicts playerId from party
    local $packet, $packetSize
    
    $packet = '1F00'
    $packet &= _hex($playerId)
    $packetSize = 6
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func startNpcDialogue($npcId, $pid)
    ;//Opens up an NPC's main menu. Is necessary before
    ;//accepting/handing in quests, buy/sell/repair
    local $packet, $packetSize
    
    $packet = '2300'
    $packet &= _hex($npcId)
    $packetSize = 6
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func useItem($index, $itemTypeId, $pid, $equip=0)
    ;//uses the item located at index. By default inventory index
    ;//is used. If equip=1, then equipment index is used. This
    ;//is necessary when toggling fly mode, as your fly gear
    ;//is then used. 
    ;//itemTypeId is the id for 
    ;//the type of item it is. This would be the same as the last
    ;//part in the url on pwdatabase. example:
    ;//http://www.pwdatabase.com/pwi/items/3044
    ;//the itemTypeId for gold is 3044.
    local $packet, $packetSize

    $packet = '2800'
    $packet &= _hex($equip, 2)
    $packet &= '01'
    $packet &= _hex($index, 2)
    $packet &= '00'
    $packet &= _hex($itemTypeId)
    
    $packetSize = 10
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func useSkill($skillId, $targetId, $pid)
    ;//uses the specified skill on the target. Pass your own
    ;//Id if you wish to use buffs. When teleporting targetId
    ;//is the targeted city.
    local $packet, $packetSize

    $packet = '2900'
    $packet &= _hex($skillId)
    $packet &= '0001'
    $packet &= _hex($targetId)
    
    $packetSize = 12
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func cancelAction($pid)
    ;//Cancels for example your current skillCast
    local $packet, $packetSize

    $packet = '2A00'
    
    $packetSize = 2
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func startMeditating($pid)
    ;//Starts meditating for faster HP/MP regen
    local $packet, $packetSize

    $packet = '2E00'
    
    $packetSize = 2
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func stopMeditating($pid)
    ;//Stop meditating for faster HP/MP regen
    local $packet, $packetSize

    $packet = '2F00'
    
    $packetSize = 2
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func useEmotion($emoteIndex, $pid)
    ;//uses the emotion located at index emoteIndex 0 to 26
    local $packet, $packetSize

    $packet = '3000'
    $packet &= _hex($emoteIndex, 4)
    
    $packetSize = 4
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func beIntimate($pid)
    ;//Uses the kissing / intimate emote when cuddling.
    local $packet, $packetSize

    $packet = '3000'
    $packet &= '1D00'
    
    $packetSize = 4
    
    sendPacket($packet, $packetSize, $pid)
EndFunc



Func swapItemInBank($bankIndex1, $bankIndex2, $pid)
    ;//swaps the location of two stacks in bank. bankIndex runs
    ;//from 0, topleft,  to 15, bottomright, in a standard non 
    ;//upgraded bank.
    local $packet, $packetSize

    $packet = '3800'
    $packet &= '03'
    $packet &= _hex($bankIndex1, 2)
    $packet &= _hex($bankIndex2, 2)
    
    $packetSize = 5
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func splitStackItemInBank($bankIndexSource, $bankIndexDestination, $amount, $pid)
    ;//Splits a stack in your bank located at bankIndexSource
    ;//Take off $amouunt from the stack and place them at bankIndexDestination
    ;//The index for a standard unexpanded bank runs from 
    ;//0, top left, to 15, bottom right    
    local $packet, $packetSize
    
    $packet = '3900'
    $packet &= '03'
    $packet &= _hex($bankIndexSource, 2)
    $packet &= _hex($bankIndexDestination, 2)
    $packet &= _hex($amount, 4)
    
    $packetSize = 7
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func swapItemBankAndInv($bankIndex, $invIndex, $pid)
    ;//Swaps a stack in your bank located at bankIndex 
    ;//with one in your inventory located at invIndex
    local $packet, $packetSize
    
    $packet = '3A00'
    $packet &= '03'
    $packet &= _hex($bankIndex, 2)
    $packet &= _hex($invIndex, 2)
    
    $packetSize = 5
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func splitStackItemInBankToInv($bankIndexSource, $invIndexDestination, $amount, $pid)
    ;//Splits a stack in your bank located at bankIndexSource
    ;//Take off $amouunt from the stack and place them at invIndexDestination
    local $packet, $packetSize
    
    $packet = '3B00'
    $packet &= '03'
    $packet &= _hex($bankIndexSource, 2)
    $packet &= _hex($invIndexDestination, 2)
    $packet &= _hex($amount, 4)
    
    $packetSize = 7
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func splitStackItemInInvToBank($invIndexSource, $bankIndexDestination, $amount, $pid)
    ;//Splits a stack in your inventory located at invIndexSource
    ;//Take off $amouunt from the stack and place them at bankIndexDestination
    local $packet, $packetSize
    
    $packet = '3C00'
    $packet &= '03'
    $packet &= _hex($invIndexSource, 2)
    $packet &= _hex($bankIndexDestination, 2)
    $packet &= _hex($amount, 4)
    
    $packetSize = 7
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func setPartySearchSettings($recruit, $jobId, $lvl, $slogan, $pid)
    ;//Changes party searching settings. 
    ;//recruit=1 means recruiting party
    ;//recruit=0 means searching for party
    ;//jobId=0 & lvl=0 & no slogan -> slogan=0x0
    ;//jobId<>0 & lvl<>0 & no slogan -> slogan=0x40
    ;//jobId=0 & lvl=0 & slogan -> slogan=0x80
    ;//jobId<>0 & lvl<> 0 & slogan -> slogan=0xC0
    local $packet, $packetSize
    
    $packet = '3F00'
    $packet &= _hex($jobId, 2)
    $packet &= _hex($lvl, 2)
    $packet &= _hex($recruit, 2)
    $packet &= _hex($slogan, 2)
    $packet &= '00000000'
    
    $packetSize = 10
    
    sendPacket($packet, $packetSize, $pid)
EndFunc


Func shiftPartyCaptain($playerId, $pid)
    ;//Shifts party captain position to playerId
    local $packet, $packetSize
    
    $packet = '4800'
    $packet &= _hex($playerId)
    $packetSize = 6
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func useSkillWithoutCastTime($skillId, $targetId, $pid)
    ;//uses the specified skill on the target. This function is used
    ;// instead of the regular skill use one for skills such as 
    ;// change to fox/tiger form or the speed buff skills. Pass your own
    ;//Id if you wish to use buffs. 
    local $packet, $packetSize

    $packet = '5000'
    $packet &= _hex($skillId)
    $packet &= '0001'
    $packet &= _hex($targetId)
    
    $packetSize = 12
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func initiateSettingUpCatShop($pid)
    ;//Starts setting up cat shop. This function is needed
    ;//before setting up the catshop.
    local $packet, $packetSize

    $packet = '5400'
    
    $packetSize = 2
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func toggleFashionDisplay($pid)
    ;//Switches between fashion and regular appearance.
    local $packet, $packetSize

    $packet = '5500'
    
    $packetSize = 2
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func acceptRez($pid)
    ;//Accept rez by a priest.
    local $packet, $packetSize

    $packet = '5700'
    
    $packetSize = 2
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func increaseFlySpeed($start, $pid)
    ;//If start=1, start faster flying. 
    ;//If start=0, stop faster flying
    local $packet, $packetSize

    $packet = '5A00'
    $packet &= _hex($start)
    
    $packetSize = 6
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func askMaleToCarry($playerId, $pid)
    ;//WHen female use this to ask a male playerId to carry you
    local $packet, $packetSize

    $packet = '5E00'
    $packet &= _hex($playerId)
    
    $packetSize = 6
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func askFemaleToBeCarried($playerId, $pid)
    ;//WHen female use this to ask a female playerId to be carried
    local $packet, $packetSize

    $packet = '5F00'
    $packet &= _hex($playerId)
    
    $packetSize = 6
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func acceptRequestByFemaleToBeCarried($playerId, $pid)
    ;//When female asks you to carry her use this to accept
    local $packet, $packetSize

    $packet = '6000'
    $packet &= _hex($playerId)
    $packet &= '00000000'

    $packetSize = 10
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func acceptRequestByMaleToCarryYou($playerId, $pid)
    ;//When male asks you if you want to be carried, use this to accept.
    local $packet, $packetSize

    $packet = '6100'
    $packet &= _hex($playerId)
    $packet &= '00000000'

    $packetSize = 10
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func releaseCarryMode($pid)
    ;//Stop carrying / being carried
    local $packet, $packetSize

    $packet = '6200'

    $packetSize = 2
    
    sendPacket($packet, $packetSize, $pid)
EndFunc



Func summonPet($petIndex, $pid)
    ;//summons pet at index petIndex. petIndex runs from 
    ;//0 to 9, depending on how many slots you have unlocked
    local $packet, $packetSize

    $packet = '6400'
    $packet &= _hex($petIndex)
    
    $packetSize = 6
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func recallPet($pid)
    ;//recalls your currently summoned pet
    local $packet, $packetSize

    $packet = '6500'
    
    $packetSize = 2
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func setPetMode($petMode, $pid)
    ;//Sets the pet to the specified mode:
    ;//petMode=0 -> defensive
    ;//petMode=1 -> attack
    ;//petMode=2 -> manual
    local $packet, $packetSize

    $packet = '6700'
    $packet &= '00000000'
    $packet &= '03000000'
    $packet &= _hex($petMode)
    
    $packetSize = 14
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func setPetFollow($pid)
    ;//Pet follows the owner
    local $packet, $packetSize

    $packet = '6700'
    $packet &= '00000000'
    $packet &= '02000000'
    $packet &= '00000000'

    $packetSize = 14
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func setPetStop($pid)
    ;//Pet stops doing whatever it was doing
    local $packet, $packetSize

    $packet = '6700'
    $packet &= '00000000'
    $packet &= '02000000'
    $packet &= '01000000'

    $packetSize = 14
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func setPetAttack($targetId, $pid)
    ;//Sets pet to do standard attacks on the target.
    local $packet, $packetSize

    $packet = '6700'
    $packet &= _hex($targetId)
    $packet &= '01'
    $packet &= '00000000'

    $packetSize = 11
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func setPetUseSkill($targetId, $skillId, $pid)
    ;//Uses skillId on the targetId. Walks up to target if out of range.
    local $packet, $packetSize

    $packet = '6700'
    $packet &= _hex($targetId)
    $packet &= '04000000'
    $packet &= _hex($skillId)
    $packet &= '00'

    $packetSize = 15
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func setPetStandardSkill($skillId, $pid)
    ;//Sets skillId to be the skill the pet uses whenever
    ;//it is cooled down
    local $packet, $packetSize

    $packet = '6700'
    $packet &= '00000000'
    $packet &= '05000000'
    $packet &= _hex($skillId)

    $packetSize = 14
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func useGenieSkill($skillId,$targetId, $pid)
    ;//Uses skillId on the target
    local $packet, $packetSize

    $packet = '7400'
    $packet &= _hex($skillId, 4)
    $packet &= '0001'
    $packet &= _hex($targetId)

    $packetSize = 10
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func feedEquippedGenie($invIndex, $amount, $pid)
    ;//Feeds the equipped genie the amount indicated from
    ;//inv index 
    local $packet, $packetSize

    $packet = '7500'
    $packet &= _hex($invIndex, 2)
    $packet &= _hex($amount)

    $packetSize = 7
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func acceptQuest($questId, $pid)
    ;//Accept a new quest
    local $packet, $packetSize

    $packet = '2500'
    $packet &= '07000000'
    $packet &= '04000000'
    $packet &= _hex($questId)

    $packetSize = 14
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func handInQuest($questId,$optionIndex, $pid)
    ;//Hand in quest, select reward optionIndex, 
    ;//which runs from 0 for first option, to more.
    local $packet, $packetSize

    $packet = '2500'
    $packet &= '06000000'
    $packet &= '08000000'
    $packet &= _hex($questId)
    $packet &= _hex($optionIndex)

    $packetSize = 18
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func sellItem($itemTypeId,$invIndex,$amount, $pid)
    ;//Sell $amount of items of type itemTypeId, located at invIndex
    ;//This function could be expanded to include selling multiple items
    ;//simultaneously. This would require setting nBytes equal to 
    ;//4 + 12 * nDifferent items. Add the extra items on the same way
    ;//as the first item.
    local $packet, $packetSize

    $packet = '2500'
    $packet &= '02000000'
    $packet &= '10000000' ;//nBytes following
    $packet &= '01000000' ;//nDifferent items being sold
    $packet &= _hex($itemTypeId)
    $packet &= _hex($invIndex)
    $packet &= _hex($amount)

    $packetSize = 26
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func buyItem($itemTypeId,$shopIndex,$amount, $pid)
    ;//Buy $amount of items of type itemTypeId, located at shopIndex
    ;//shopIndex is calculated as follows:
    ;//Each tab in the shop has 32 available spaces, index of each space 
    ;//starts at 0, index of each tab starts at 0. $shopIndex would then be 
    ;//shopIndex = tabIndex * 32 + spaceIndex
    ;//This function could be expanded to include buying multiple items
    ;//simultaneously. This would require setting nBytes equal to 
    ;//8 + 12 * nDifferent items. Add the extra items on the same way
    ;//as the first item.
    local $packet, $packetSize

    $packet = '2500'
    $packet &= '01000000'
    $packet &= '14000000' ;//nBytes following
    $packet &= '00000000'
    $packet &= '01000000' ;//nDifferent items being bought
    $packet &= _hex($itemTypeId)
    $packet &= _hex($shopIndex)
    $packet &= _hex($amount)

    $packetSize = 30
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func repairAll($pid)
    ;//Repair all items
    local $packet, $packetSize

    $packet = '2500'
    $packet &= '03000000'
    $packet &= '06000000' 
    $packet &= 'FFFFFFFF'
    $packet &= '0000' 

    $packetSize = 16
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func repairItem($itemTypeId, $isEquipped, $locationIndex, $pid)
    ;//repairs the item of type itemTypeId at locationIndex, if 
    ;//isEquipped=1, location refers to equipment. If isEquipped=0,
    ;//location refers to inventory.
    local $packet, $packetSize

    $packet = '2500'
    $packet &= '03000000'
    $packet &= '06000000' 
    $packet &= _hex($itemTypeId)
    $packet &= _hex($isEquipped, 2)
    $packet &= _hex($locationIndex, 2)

    $packetSize = 16
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func upgradeSkill($skillId, $pid)
    ;//Upgrades the requested skill by one level
    local $packet, $packetSize

    $packet = '2500'
    $packet &= '09000000'
    $packet &= '04000000' 
    $packet &= _hex($skillId)

    $packetSize = 14
    
    sendPacket($packet, $packetSize, $pid)
EndFunc

Func sendPacket($packet, $packetSize, $pid)
    ;//Declare local variables
    Local $pRemoteThread, $vBuffer, $loop, $result, $OPcode, $processHandle, $packetAddress
    
    ;//Open process for given processId
    $processHandle = memopen($pid)
    
    ;//Allocate memory for the OpCode and retrieve address for this
    $functionAddress = DllCall($kernel32, 'int', 'VirtualAllocEx', 'int', $processHandle, 'ptr', 0, 'int', 0x46, 'int', 0x1000, 'int', 0x40)
    
    ;//Allocate memory for the packet to be sent and retrieve the address for this
    $packetAddress = DllCall($kernel32, 'int', 'VirtualAllocEx', 'int', $processHandle, 'ptr', 0, 'int', $packetSize, 'int', 0x1000, 'int', 0x40)
    
    ;//Construct the OpCode for calling the 'SendPacket' function
    $OPcode &= '60'                                ;//PUSHAD
    $OPcode &= 'B8'&_hex($sendPacketFunction)    ;//MOV     EAX, sendPacketAddress
    $OPcode &= '8B0D'&_hex($realBaseAddress)    ;//MOV     ECX, DWORD PTR [revBaseAddress]
    $OPcode &= '8B4920'                            ;//MOV     ECX, DWORD PTR [ECX+20]
    $OPcode &= 'BF'&_hex($packetAddress[0])        ;//MOV     EDI, packetAddress    //src pointer
    $OPcode &= '6A'&_hex($packetSize,2)            ;//PUSH    packetSize        //size
    $OPcode &= '57'                                ;//PUSH    EDI
    $OPcode &= 'FFD0'                            ;//CALL    EAX
    $OPcode &= '61'                                ;//POPAD
    $OPcode &= 'C3'                                ;//RET        
    
    ;//Put the OpCode into a struct for later memory writing
    $vBuffer = DllStructCreate('byte[' & StringLen($OPcode) / 2 & ']')
    For $loop = 1 To DllStructGetSize($vBuffer)
        DllStructSetData($vBuffer, 1, Dec(StringMid($OPcode, ($loop - 1) * 2 + 1, 2)), $loop)
    Next
    
    ;//Write the OpCode to previously allocated memory
    DllCall($kernel32, 'int', 'WriteProcessMemory', 'int', $processHandle, 'int', $functionAddress[0], 'int', DllStructGetPtr($vBuffer), 'int', DllStructGetSize($vBuffer), 'int', 0)
        
    ;//Put the packet into a struct for later memory writing
    $vBuffer = DllStructCreate('byte[' & StringLen($packet) / 2 & ']')
    For $loop = 1 To DllStructGetSize($vBuffer)
        DllStructSetData($vBuffer, 1, Dec(StringMid($packet, ($loop - 1) * 2 + 1, 2)), $loop)
    Next
    
    ;//Write the packet to previously allocated memory
    DllCall($kernel32, 'int', 'WriteProcessMemory', 'int', $processHandle, 'int', $packetAddress[0], 'int', DllStructGetPtr($vBuffer), 'int', DllStructGetSize($vBuffer), 'int', 0)
        
    ;//Create a remote thread in order to run the OpCode
    $hRemoteThread = DllCall($kernel32, 'int', 'CreateRemoteThread', 'int', $processHandle, 'int', 0, 'int', 0, 'int', $functionAddress[0], 'ptr', 0, 'int', 0, 'int', 0)
    
    ;//Wait for the remote thread to finish
    Do
        $result = DllCall('kernel32.dll', 'int', 'WaitForSingleObject', 'int', $hRemoteThread[0], 'int', 50)
    Until $result[0] <> 258
    
    ;//Close the handle to the previously created remote thread
    DllCall($kernel32, 'int', 'CloseHandle', 'int', $hRemoteThread[0])
    
    ;//Free the previously allocated memory
    DllCall($kernel32, 'ptr', 'VirtualFreeEx', 'hwnd', $processHandle, 'int', $functionAddress[0], 'int', 0, 'int', 0x8000)
    DllCall($kernel32, 'ptr', 'VirtualFreeEx', 'hwnd', $processHandle, 'int', $packetAddress[0], 'int', 0, 'int', 0x8000)
    
    ;//Close the Process
    memclose($processHandle)
    
    Return True
EndFunc

Func memopen($pid)
    Local $mid = DllCall($kernel32, 'int', 'OpenProcess', 'int', 0x1F0FFF, 'int', 1, 'int', $pid)
    Return $mid[0]
EndFunc

Func memclose($mid)
    DllCall($kernel32, 'int', 'CloseHandle', 'int', $mid)
EndFunc

Func _hex($Value, $size=8)
    Local $tmp1, $tmp2, $i 
    $tmp1 = StringRight("000000000" & Hex($Value),$size) 
    For $i = 0 To StringLen($tmp1) / 2 - 1 
        $tmp2 = $tmp2 & StringMid($tmp1, StringLen($tmp1) - 1 - 2 * $i, 2)
    Next
    Return $tmp2
EndFunc 