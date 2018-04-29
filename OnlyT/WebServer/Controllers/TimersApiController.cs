﻿namespace OnlyT.WebServer.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using ErrorHandling;
    using Models;
    using OnlyT.Models;
    using Services.TalkSchedule;
    using Services.Timer;

    internal class TimersApiController : BaseApiController
    {
        private readonly ITalkTimerService _timerService;
        private readonly ITalkScheduleService _talkScheduleService;

        public TimersApiController(ITalkTimerService timerService, ITalkScheduleService talkScheduleService)
        {
            _timerService = timerService;
            _talkScheduleService = talkScheduleService;
        }

        public void Handler(HttpListenerRequest request, HttpListenerResponse response)
        {
            CheckMethodGetOrPost(request);

            if (IsMethodGet(request))
            {
                HandleGetTimersApi(request, response);
            }
            else if (IsMethodPost(request))
            {
                HandlePostTimersApi(request, response);
            }
        }

        private void HandlePostTimersApi(HttpListenerRequest request, HttpListenerResponse response)
        {
            throw new NotImplementedException();
        }

        private void HandleGetTimersApi(HttpListenerRequest request, HttpListenerResponse response)
        {
            CheckSegmentLength(request, 4, 5);

            var allTimerData = GetTimersInfo();

            switch (request.Url.Segments.Length)
            {
                case 4:
                    // segments: "/" "api/" "v1/" "timers/"
                    // request for _all_ timer info...
                    WriteResponse(response, allTimerData);
                    break;

                case 5:
                    // segments: "/" "api/" "v1/" "timers/" "0/"
                    // gets info for a single timer...
                    if (int.TryParse(request.Url.Segments[4], out var index))
                    {
                        WriteResponse(response, GetTimerInfo(allTimerData, index));
                    }

                    break;
            }
        }

        private TimerInfo GetTimerInfo(TimersResponseData allTimerData, int timerId)
        {
            var result = allTimerData.TimerInfo.SingleOrDefault(x => x.Id == timerId);
            if (result == null)
            {
                throw new WebServerException(WebServerErrorCode.TimerDoesNotExist);
            }

            return result;
        }

        private TimersResponseData GetTimersInfo()
        {
            var result = new TimersResponseData();

            var talks = _talkScheduleService.GetTalkScheduleItems();
            
            foreach (var talk in talks)
            {
                result.Add(CreateTimerInfo(talk));
            }

            return result;
        }

        private TimerInfo CreateTimerInfo(TalkScheduleItem talk)
        {
            return new TimerInfo
            {
                Id = talk.Id,
                LocalisedTitle = talk.Name,
                OriginalDurationSecs = (int)talk.OriginalDuration.TotalSeconds,
                ModifiedDurationSecs = talk.ModifiedDuration == null 
                    ? null 
                    : (int?)talk.ModifiedDuration.Value.TotalSeconds,
                AdaptedDurationSecs = talk.AdaptedDuration == null
                    ? null
                    : (int?)talk.AdaptedDuration.Value.TotalSeconds,
                ActualDurationSecs = (int)talk.ActualDuration.TotalSeconds,
                UsesBell = talk.Bell
            };
        }
    }
}
