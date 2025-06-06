import { useTranslation } from "react-i18next";
import { Toaster, toaster } from "@/components/ui/toaster";
import { Box, Button, FileUpload, Icon, } from "@chakra-ui/react"
import { LuUpload } from "react-icons/lu"
import { useState } from "react";

const maxFiles = 1;
const maxFileSize = 10485760; // 10MB

export function FileManager({ allowedTypes, onUpload }) {
    const { t } = useTranslation();

    function validateFileType(file, details) {
        if (allowedTypes?.some(e => file.name.endsWith(e))) {
            details.acceptedFiles = [...details.acceptedFiles, file];
            return null;
        }

        return ["forbiddenFileType"];
    }

    function fileRejected(details) {
        const errorType = details.files[0].errors[0] === "FILE_TOO_LARGE" ? "fileTooLarge" : details.files[0]?.errors[0];

        toaster.create({
            title: t(`errors.${errorType}`, { filename: details.files[0]?.file.name }),
            type: "error",
        });
    }

    async function handleUpload(files) {
        if (!files.length) return;
        onUpload(files[0])
    }

    return (
        <Box w={"full"}>
            <FileUpload.Root onFileReject={fileRejected} validate={validateFileType} minFileSize={1} maxFileSize={maxFileSize} maxFiles={maxFiles} w="full">
                <Toaster />
                <FileUpload.HiddenInput />
                <FileUpload.Dropzone>
                    {/* <Icon size="md" color="fg.muted"> */}
                        <LuUpload />
                    {/* </Icon> */}
                    <FileUpload.DropzoneContent>
                        <Box>{t("fields.labels.dragFile")}</Box>
                        <Box color="fg.muted">{t("fields.labels.fileConstraint")}</Box>
                    </FileUpload.DropzoneContent>
                </FileUpload.Dropzone>
                <FileUpload.ItemGroup>
                    <FileUpload.Context>
                        {({ acceptedFiles }) => (
                            <>
                                {acceptedFiles.map((file) => (
                                    <FileUpload.Item key={file.name} file={file}>
                                        <FileUpload.ItemPreview />
                                        <FileUpload.ItemName />
                                        <FileUpload.ItemSizeText />
                                        <FileUpload.ItemDeleteTrigger />
                                    </FileUpload.Item>
                                ))}
                                <Button mt={3} className="upload-file" bg="primary" onClick={() => handleUpload(acceptedFiles)} disabled={acceptedFiles.length < 0}>
                                    {t("fields.labels.upload")}
                                </Button>
                            </>
                        )}
                    </FileUpload.Context>
                </FileUpload.ItemGroup>
            </FileUpload.Root>
        </Box>)

}