import { Box, Code, FileUpload, Icon, InputGroup, useFileUpload } from "@chakra-ui/react"
import { useTranslation } from "react-i18next"
import { LuUpload } from "react-icons/lu"
import { Toaster, toaster } from "@/components/ui/toaster"

const maxFiles = 10;
const maxFileSize = 10485760 // 10MB

export function FileManager({ allowedTypes }) {
    const { t } = useTranslation()

    function validatFiletype(file, details) {
        //console.log(file, details);

        if (allowedTypes ?? [] .some(e => file.name.endsWith(e))) {
            details.acceptedFiles = [...details.acceptedFiles, file];
            return null;
        }

        return [
            "forbiddenFileType"
        ];

    }

    function fileRejected(details) {
        console.log(details)

        const errorType = details.files[0].errors[0] == "FILE_TOO_LARGE" ? "fileTooLarge" :
            details.files[0]?.errors[0]

        toaster.create({
            title: t(`errors.${errorType}`, { filename: details.files[0]?.file.name }),
            type: "error"
        })
    }

    return (
        <FileUpload.Root
            onFileReject={fileRejected}
            validate={validatFiletype}
            maxFileSize={maxFileSize}
            maxW="xl" alignItems="stretch"
            maxFiles={maxFiles}>
            <Toaster />
            <FileUpload.HiddenInput />
            <FileUpload.Dropzone>
                <Icon size="md" color="fg.muted">
                    <LuUpload />
                </Icon>
                <FileUpload.DropzoneContent>
                    <Box>{t("fields.labels.dragFile")}</Box>
                    <Box color="fg.muted">{t("fields.labels.fileConstraint")}</Box>
                </FileUpload.DropzoneContent>
            </FileUpload.Dropzone>
            <FileUpload.ItemGroup>
                <FileUpload.Context>
                    {({ acceptedFiles }) =>
                        acceptedFiles.map((file) => (
                            <FileUpload.Item key={file.name} file={file}>
                                <FileUpload.ItemPreview />
                                <FileUpload.ItemName />
                                <FileUpload.ItemSizeText />
                                <FileUpload.ItemDeleteTrigger />
                            </FileUpload.Item>
                        ))
                    }
                </FileUpload.Context>
            </FileUpload.ItemGroup>
        </FileUpload.Root>

    )
}