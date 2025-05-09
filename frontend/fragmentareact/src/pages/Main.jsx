import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Sidebar } from '../components/Sidebar'
import { api } from "../api/fetchClient.js"
import { WorkspaceProvider } from '@/utils/WorkspaceContext'
import { Workspace } from './Workspace.jsx'
import { useUser } from '@/utils/UserContext'
import { Box, Heading } from '@chakra-ui/react'

export function Main() {

  const { userName } = useUser()
  const { t } = useTranslation();

  function getTimeBasedGreeting(t, username) {
    const hour = new Date().getHours();

    // Визначаємо час доби
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
      <Box p={4}>
        <Heading>
          {getTimeBasedGreeting(t, userName)}
        </Heading>
      </Box>

    </div>
  )
}