﻿using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using VagtplanApp.Shared.Model;

namespace VagtplanApp.Client.Services
{
    public class ShiftService : IShiftService
    {
        // HTTP klient bruges til at lave web requests
        private readonly HttpClient httpClient;
        // LocalStorage bruges til at gemme og hente data lokalt
        private readonly ILocalStorageService localStorage;

        public ShiftService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            this.httpClient = httpClient;
            this.localStorage = localStorage;
        }

        // Henter alle vagter fra serveren
        public async Task<List<Shift>> GetAllShifts()
        {
            var shifts = await httpClient.GetFromJsonAsync<List<Shift>>("api/shift/getall");

            return shifts;
        }

        // Sortere alle shifts på prioritet
        public List<Shift> GetSortedShifts(List<Shift> shifts, bool sortByPriority)
        {
            return sortByPriority
                ? shifts.OrderBy(shift => shift.priority).ToList()
                : shifts.ToList();
        }

        // Opretter en ny vagt ved at sende data til serveren
        public async Task CreateShift(Shift shift)
        {
            await httpClient.PostAsJsonAsync("api/shift/add", shift);
        }

        // Frivillige kan tage vagter
        public async Task<bool> TakeShift(string shiftId)
        {
            // Henter den nuværende bruger fra LocalStorage
            var currentUser = await localStorage.GetItemAsync<Person>("currentUser");

            // Sender en anmodning til serveren for at tage en vagt
            var response = await httpClient.PutAsJsonAsync($"/api/shift/takeshift/{shiftId}", currentUser.id);

            // Returnerer resultatet af anmodningen
            return response.IsSuccessStatusCode;
        }


        // Henter vagter for den aktuelle bruger
        public async Task<List<Shift>> GetShiftsForVolunteer()
        {
            try
            {
                var currentUser = await localStorage.GetItemAsync<Person>("currentUser");
                if (currentUser == null) return new List<Shift>();

                var shifts = await httpClient.GetFromJsonAsync<List<Shift>>($"api/shift/person/{currentUser.id}");
                return shifts ?? new List<Shift>();
            }
            catch (Exception ex)
            {
                // Log fejl eller håndter den på anden måde
                return new List<Shift>();
            }
        }

        // Personer kan fjerne sig selv fra en vagt
        public async Task RemovePersonFromShift(string shiftId)
        {
            var currentUser = await localStorage.GetItemAsync<Person>("currentUser");

            // Sender en anmodning til serveren for at fjerne den nuværende bruger fra en vagt
            await httpClient.PutAsync($"api/shift/removeperson/{shiftId}/{currentUser.id}", null);

        }

        public async Task UpdateShift(Shift updatedShift)
        {
            var response = await httpClient.PutAsJsonAsync("api/shift/updateshift", updatedShift);
        }

        private bool CheckTimeOverlap(DateTime startCurrentShift, DateTime endCurrentShift, DateTime startAnyShift, DateTime endAnyShift)
        {
            return startCurrentShift < endAnyShift && endCurrentShift > startAnyShift;
        }

        public async Task<Shift> GetShiftById(string shiftId)
        {
            // Antagelse: Henter en specifik vagt fra serveren ved at bruge shiftId
            return await httpClient.GetFromJsonAsync<Shift>($"api/shift/{shiftId}");
        }

        public async Task<bool> TryTakeShift(string shiftId)
        {
            try
            {
                var currentUser = await localStorage.GetItemAsync<Person>("currentUser");
                if (currentUser == null) return false;

                var currentShift = await GetShiftById(shiftId);
                if (currentShift == null) return false;

                var takenUserShifts = await GetShiftsForVolunteer();
                foreach (var shift in takenUserShifts)
                {
                    if (CheckTimeOverlap(currentShift.startTime, currentShift.endTime, shift.startTime, shift.endTime))
                    {
                        return false;
                    }
                }

                return await TakeShift(shiftId);
            }
            catch (Exception ex)
            {
                // Log fejl eller håndter den på anden måde
                return false;
            }
        }
    }
}
