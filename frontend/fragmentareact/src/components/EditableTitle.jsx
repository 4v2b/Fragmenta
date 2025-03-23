import { LuFolder, LuCheck, LuPencilLine, LuX, LuUser } from "react-icons/lu"
import { Editable, IconButton, } from "@chakra-ui/react"
import { useState } from "react"

export function EditableTitle({ canEdit, content, onContentEdit }) {
    //const [value, setValue] = useState(content ?? "")

    console.log(content)
    return <> {canEdit ? (<Editable.Root submitMode="enter" width={"fit-content"} onValueCommit={e => onContentEdit(e.value) } defaultValue={content}>
        <Editable.Preview />
        <Editable.Input />
        <Editable.Control>
            <Editable.EditTrigger asChild>
                <IconButton variant="ghost" size="xs">
                    <LuPencilLine />
                </IconButton>
            </Editable.EditTrigger>
            <Editable.CancelTrigger asChild>
                <IconButton variant="outline" size="xs">
                    <LuX />
                </IconButton>
            </Editable.CancelTrigger>
            <Editable.SubmitTrigger asChild>
                <IconButton variant="outline" size="xs">
                    <LuCheck />
                </IconButton>
            </Editable.SubmitTrigger>
        </Editable.Control>
    </Editable.Root>) : content}</>
}