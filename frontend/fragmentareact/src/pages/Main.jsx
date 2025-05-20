import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Sidebar } from '../components/Sidebar'
import { api } from "../api/fetchClient.js"
import { WorkspaceProvider } from '@/utils/WorkspaceContext'
import { Workspace } from './Workspace.jsx'
import { useUser } from '@/utils/UserContext'
import { useNavigate, useOutletContext } from 'react-router'
import {
  Box, Heading, Text, Stack, Button,
  HStack,
  Badge
} from '@chakra-ui/react';
import { LuClipboardCheck, LuClipboardList, LuFolderOpen, LuUser } from 'react-icons/lu'
import { formatDistanceToNow } from 'date-fns';
import { uk, enUS } from 'date-fns/locale';
import i18n from '@/i18n'

export function Main() {
  const { items } = useOutletContext();
  const { userName } = useUser()
  const { t } = useTranslation();
  const navigate = useNavigate()

  function getTimeBasedGreeting(t, username) {
    const hour = new Date().getHours();

    let greeting;
    if (hour >= 5 && hour < 12) {
      greeting = t('greetings.morning', { username });
    } else if (hour >= 12 && hour < 18) {
      greeting = t('greetings.afternoon', { username });
    } else if (hour >= 18 && hour < 23) {
      greeting = t('greetings.evening', { username });
    } else {
      greeting = t('greetings.night', { username });
    }

    return greeting;
  };

  const recentBoards = JSON.parse(localStorage.getItem("recentBoards") || "[]");

  return (
    <div className='main-container'>

      <Stack  m={10} p={8} pb={16} gap={4} spacing={6} overflow={"auto"} bg={"background"} borderRadius={4}>
        <Box mb={6}>
          <Heading fontWeight={"semibold"} >
            {getTimeBasedGreeting(t, userName)}
          </Heading>
        </Box>

        <Box mb={6}>
          <Text fontSize="xl" mb={2}>{t("common.recent")}</Text>
          <Stack align="start" spacing={2}>
            {recentBoards.map(b => (
              <HStack _hover={{ shadow: 'sm' }} w={"60%"} cursor={"pointer"} p={4} borderRadius="md" borderWidth="1px" key={b.boardId} onClick={() => navigate(`/workspaces/${b.workspaceId}/boards/${b.boardId}`)} variant="link">
                <LuClipboardList />{b.boardName}
                <Text ml={4} fontSize="sm" color="gray.500">
                  {t("common.lastOpened")} {formatDistanceToNow(new Date(b.openedAt), { addSuffix: true, locale: i18n.language == "uk" ? uk : enUS })}
                </Text>
                <Badge ml={4}>
                  <LuUser />
                  {t("roles." + b?.role?.toLowerCase())}
                </Badge>
              </HStack>
            ))}
          </Stack>
        </Box>

        <Box mt={6}>
          <Text fontSize="xl" mb={2}>{t("common.myWorkspaces")}</Text>
          <Stack align="start" spacing={2}>
            {items?.map(w => (
              <HStack _hover={{ shadow: 'sm' }} w={"60%"} cursor={"pointer"} p={4} borderRadius="md" borderWidth="1px" key={w.id} onClick={() => navigate(`/workspaces/${w.id}`)} variant="link">
                <LuFolderOpen />{w.name}
              </HStack>
            ))}
          </Stack>
        </Box>

      </Stack>
    </div>
  )
}