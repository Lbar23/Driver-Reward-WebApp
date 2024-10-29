//Where any and all authContexts are/can go; and any axios global changes...
//Can update this to include login at a later date...
import axios from 'axios';

export const authService = {
  async logout() {
    try {
      await axios.post('/api/user/logout', {}, { 
            withCredentials: true //Make sure cookies are included in the request;
        });

      // Clear any local storage/state
      localStorage.clear();

      //Redirect to login page
      window.location.href = '/login';
    } catch (error) {
      console.error('Logout failed:', error);
      throw error;
    }
  }
};