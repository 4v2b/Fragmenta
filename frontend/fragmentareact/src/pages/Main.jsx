import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Sidebar } from '../components/Sidebar'
import { api } from "../api/fetchClient.js"
import { WorkspaceProvider } from '@/utils/WorkspaceContext'
import { Workspace } from './Workspace.jsx'
import { useUser } from '@/utils/UserContext'
import { Box, Heading, Stack } from '@chakra-ui/react'
import { useOutletContext } from 'react-router'

export function Main() {
  const { items } = useOutletContext();
  const { userName } = useUser()
  const { t } = useTranslation();

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

  return (
    <div className='main-container'>

      <Stack m={8} p={8} gap={4} spacing={6} overflow={"auto"} bg={"background"} borderRadius={4}>
        <Box p={4}>
          <Heading fontWeight={"bold"} >
            {getTimeBasedGreeting(t, userName)}
          </Heading>
        </Box>

        <Box p={4}>
          <Heading  fontWeight={"medium"} size={"lg"}>
            Last item
          </Heading>
        </Box>

        <Box p={4}>
          <Heading fontWeight={"medium"} size={"lg"}>
            Your workspaces
          </Heading>

          {items?.filter(w => w.role == 'Owner')?.map(w => (<Box>{w.name}</Box>))}
        </Box>
      </Stack>
    </div>
  )
}